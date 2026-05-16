using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Search.Core;
using Search.Levels;
using Search.Utils;
using Search.Visualization;
using Search.GameModes;
using UnityEngine;

namespace Search.Controllers
{
    public class SearchController : Singleton<SearchController>, ISearchListener
    {
        private IVisualizer problemVisualizer => LevelManager.Instance?.ProblemVisualizer;
        private TreeVisualizer treeVisualizer => !LevelManager.Instance.isMainMenu ? 
            LevelManager.Instance?.TreeVisualizer : null;

        public QueueUI queueUI;
        
        [field: SerializeField] public bool isAutomaticSearch { get; private set; } = false;
        public float problemNodeCreationAnimationDuration = 1f;
        public float treeNodeCreationAnimationDuration = 1f;

        private Coroutine _pollAutomationCoroutine;
        private Coroutine _solutionPathCoroutine;
        public float searchTimeScale = 1f;

        // List of node visuals that need to be set to frontier state once
        // the player has advanced the step (ordered the frontier correctly) :)
        private readonly List<NodeVisual> _nodeVisualsToFrontier = new();

        private void OnEnable()
        {
            LevelManager.Instance?.onSearchLoaded.AddListener(OnSearchLoaded);
        }

        private void OnDisable()
        {
            LevelManager.Instance.onSearchLoaded.RemoveListener(OnSearchLoaded);
        }

        /// <summary>
        /// Only clears the collection of node visuals that are waiting to be set to frontier.
        /// Should only be called when switching modes or when a new search is loaded
        /// </summary>
        public void ClearState()
        {
            _nodeVisualsToFrontier.Clear();
        }
        
        private void OnSearchLoaded()
        {
            if(_solutionPathCoroutine != null)
                StopCoroutine(_solutionPathCoroutine);
            _solutionPathCoroutine = null;
            
            ClearState();
            SearchModeController.Instance.ResetSolveState();
        }

        public void OnNodeExpanded(SearchNode node)
        {
            var problemVisual = problemVisualizer?.GetOrCreateNodeVisual(node, node.parent);
                problemVisual?.SetState(NodeState.Expanded);
                
            var treeVisual = treeVisualizer?.GetOrCreateNodeVisual(node, node.parent);
                treeVisual?.SetState(NodeState.Expanded);
            
            var nodeVisual = LevelManager.Instance.useGraphSearch ? problemVisual : treeVisual;
            SearchModeController.Instance.NotifyNodeExpanded(nodeVisual);
        }

        public void OnNodeExpanding(SearchNode node)
        {
            // Don't show any nodes getting expanded in the next step. Cheating is bad
            if (SearchModeController.Instance.CurrentMode == SearchPlayMode.Solve)
                return;
            
            problemVisualizer?.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Expanding);
            problemVisualizer?.BlinkNode(node.state, Color.red);
            problemVisualizer?.TryPlaySameState(node);
            
            treeVisualizer?.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Expanding);
            treeVisualizer?.BlinkNode(node, Color.red);
            treeVisualizer?.TryPlaySameState(node);
        }

        public void OnNodeGenerated(SearchNode node)
        {
            problemVisualizer?.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Default);
            treeVisualizer?.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Default);
        }

        public void OnFrontierReordered(IReadOnlyList<SearchNode> frontier)
        {
            var levelManager = LevelManager.Instance;
            var nodeVisuals = frontier.Select(searchNode => levelManager.useGraphSearch
                    ? problemVisualizer?.GetNodeVisual(searchNode)
                    : treeVisualizer?.GetNodeVisual(searchNode))
                .Where(v => v)
                .ToList();
            queueUI?.SyncFrontierOrder(nodeVisuals);
            SearchModeController.Instance.NotifyFrontierChanged(nodeVisuals);
        }

        public void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> nodes)
        {
            var levelManager = LevelManager.Instance;
            List<NodeVisual> nodeVisuals = new();
            foreach (var node in nodes)
            { 
                var problemVisual = problemVisualizer?.GetOrCreateNodeVisual(node, node.parent);
                if(SearchModeController.Instance.CurrentMode == SearchPlayMode.Observe)
                    problemVisual?.SetState(NodeState.Frontier);
                else
                    _nodeVisualsToFrontier.Add(problemVisual);
                
                var treeVisual = treeVisualizer?.GetOrCreateNodeVisual(node, node.parent);
                if(SearchModeController.Instance.CurrentMode == SearchPlayMode.Observe)
                    treeVisual?.SetState(NodeState.Frontier);
                else
                    _nodeVisualsToFrontier.Add(treeVisual);
                
                nodeVisuals.Add(levelManager.useGraphSearch ? problemVisual : treeVisual);
            }
            queueUI?.AddNodes(nodeVisuals);
            SearchModeController.Instance.NotifyDeltaGenerated(nodeVisuals);
            SearchModeController.Instance.NotifyNodesAddedToFrontier(nodeVisuals);
        }

        public void OnSearchComplete(SearchResult result)
        {
            if (result.success)
            {
                var node = result.solutionNode;
                
                _solutionPathCoroutine = StartCoroutine(AnimateSolutionPath(node));
            }

            if (!LevelManager.Instance.isMainMenu)
            {
                SearchResultUI.Instance.DisplayResult(result);
            }
            
            result.PrintSummary();
        }

        private IEnumerator AnimateSolutionPath(SearchNode node)
        {
            while (node != null)
            {
                problemVisualizer?.GetNodeVisual(node).SetState(NodeState.Path);
                treeVisualizer?.GetNodeVisual(node).SetState(NodeState.Path);
                node = node.parent;
                if(isAutomaticSearch)
                    yield return new WaitForSeconds(0.5f/searchTimeScale);
            }
        }

        public void AddEdge(SearchNode a, SearchNode b)
        {
            var label = LevelManager.Instance.GetCurrentAlgorithm() switch
            {
                AlgorithmType.BFS or AlgorithmType.DFS or 
                    AlgorithmType.IDS or AlgorithmType.GBFS => b.actionFromParent,
                AlgorithmType.UCS or AlgorithmType.Astar => $"{b.actionFromParent}\n" +
                                                            $"{b.stepCostFromParent}",
                _ => b.actionFromParent
            };
            
            var nodeAProblem = problemVisualizer?.GetOrCreateNodeVisual(a, a.parent);
            var nodeBProblem = problemVisualizer?.GetOrCreateNodeVisual(b, b.parent);
            if(nodeAProblem && nodeBProblem)
                EdgeManager.Instance.AddEdge(a.state.id, b.state.id, nodeAProblem.transform, nodeBProblem.transform,
                    label);

            if (LevelManager.Instance.useGraphSearch && treeVisualizer?.GetNodeVisual(b))
            {
                return;
            }
            var nodeATree = treeVisualizer?.GetOrCreateNodeVisual(a, a.parent);
            var nodeBTree = treeVisualizer?.GetOrCreateNodeVisual(b, b.parent);
            
            if(!nodeATree || !nodeBTree)
                return;
            
            var uniqueIdA = a.GetHashCode().ToString();
            var uniqueIdB = b.GetHashCode().ToString();

            EdgeManager.Instance.AddEdge(uniqueIdA, uniqueIdB, nodeATree.transform, nodeBTree.transform, 
                label);
        }

        public void RemoveEdge(SearchNode a, SearchNode b)
        {
            EdgeManager.Instance.RemoveEdge(a.state.id, b.state.id);
            EdgeManager.Instance.RemoveEdge(a.state.id + "_tree", b.state.id + "_tree");
        }

        public void ResetParent(SearchNode childNode, IReadOnlyList<SearchNode> frontier)
        {
            if(!treeVisualizer)
                return;
            var label = LevelManager.Instance.GetCurrentAlgorithm() switch
            {
                AlgorithmType.BFS or AlgorithmType.DFS or 
                    AlgorithmType.IDS or AlgorithmType.GBFS => childNode.actionFromParent,
                AlgorithmType.UCS or AlgorithmType.Astar => $"{childNode.actionFromParent}\n" +
                                                            $"{childNode.stepCostFromParent}",
                _ => childNode.actionFromParent
            };
            
            treeVisualizer.ResetParent(childNode);
            var parentNodeVisual = treeVisualizer.GetOrCreateNodeVisual(childNode.parent, childNode.parent.parent);
            var childNodeVisual = treeVisualizer.GetOrCreateNodeVisual(childNode, childNode.parent);
            EdgeManager.Instance.AddEdge(childNode.parent.state.id + "_tree", childNode.state.id + "_tree",
                parentNodeVisual.transform, childNodeVisual.transform, label);
            OnFrontierReordered(frontier);
        }

        public void AdvanceStep()
        {
            var search = LevelManager.Instance.searchAlgorithm;

            if (IsAnimating())
            {
                DOTween.Complete("Search");
                return;
            }

            foreach (var nodeVisual in _nodeVisualsToFrontier)
            {
                nodeVisual.SetState(NodeState.Frontier);
            }
            
            search?.AdvanceStep();
        }
        
        public void SetAutomaticSearch(bool? value = null)
        {
            if(!value.HasValue)
                // ReSharper disable once TailRecursiveCall
                SetAutomaticSearch(isAutomaticSearch);
            else
            {
                isAutomaticSearch = value.Value;

                if (value.Value)
                {
                    if(_pollAutomationCoroutine != null)
                        StopCoroutine(_pollAutomationCoroutine);
                    
                    _pollAutomationCoroutine = StartCoroutine(PollAutomation(1f / searchTimeScale));
                }
                else
                {
                    if (_pollAutomationCoroutine != null)
                    {
                        StopCoroutine(_pollAutomationCoroutine);
                        _pollAutomationCoroutine = null;
                    }
                }
            }
        }

        public bool skipDelay;
        
        private IEnumerator PollAutomation(float delay)
        {
            while (isAutomaticSearch)
            {
                var search = LevelManager.Instance.GetCurrentSearchAlgorithm();
                yield return new WaitUntil(() => !IsAnimating() && searchTimeScale != 0);

                skipDelay = false;
                
                var elapsed = 0f;
                while (elapsed < delay && !skipDelay)
                {
                    if(!isAutomaticSearch) // in case it was turned off while waiting
                        yield break;
                    
                    yield return new WaitUntil(() => searchTimeScale > 0f);
                    elapsed += Time.unscaledDeltaTime * searchTimeScale;
                    yield return null;
                }
                
                if(!isAutomaticSearch) // in case it was turned off while waiting
                    yield break;
                
                search?.AdvanceStep();
            }
        }
        
        public bool IsAnimating() => DOTween.IsTweening("Search");

        public void SetSearchTimeScale(float timeScale)
        {
            searchTimeScale =  timeScale;
            var tweens = DOTween.TweensById("Search");

            if(tweens == null)
                return;
            
            foreach (var tween in tweens)
            {
                tween.timeScale = timeScale / tween.timeScale;
            }
        }
    }
}