using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Search.Controllers;
using Search.Levels;
using Search.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Search.Core.Algorithms
{
    [System.Serializable]
    public class GeneralSearch
    {
        private readonly IQueuingFunction _queueingFunction;

        private readonly IFrontier<SearchNode> _frontier;
        private readonly List<SearchNode> _expanded = new();
        private readonly Dictionary<IState, SearchNode> _searchNodes;

        private readonly int? _levelLimit;
        private readonly LevelManager _levelManager;
        private readonly ISearchListener _searchListener;
        private SearchResult _searchResult;

        private Stopwatch _searchTimer;
        public float ElapsedTimeinS => _searchTimer?.ElapsedMilliseconds / 1000f ?? 0f;

        private bool _stepRequested = false; // becomes true when user clicks "Next Step"

        [Tooltip("Maximum number of nodes to expand before terminating search with failure.")]
        public int expansionLimit;

        public GeneralSearch(IQueuingFunction queueingFunction, int? levelLimit = 0,
            int expansionLimit = 10000)
        {
            _levelManager = LevelManager.Instance;
            _searchListener = SearchController.Instance;
            _queueingFunction = queueingFunction;
            _searchNodes = _levelManager.useGraphSearch ? new Dictionary<IState, SearchNode>() : null;
            _levelLimit = levelLimit;
            this.expansionLimit = expansionLimit;
            _frontier = _queueingFunction switch
            {
                BFS => new Utils.Queue<SearchNode>(),
                DFS => new Utils.Stack<SearchNode>(),
                UCS => new PriorityQueue<SearchNode>((a, b) =>
                    a.pathCost.CompareTo(b.pathCost)),
                IDS => new Utils.Stack<SearchNode>(),
                GBFS => new PriorityQueue<SearchNode>((a, b) =>
                    a.heuristicCost.CompareTo(b.heuristicCost)),
                Astar => new PriorityQueue<SearchNode>((a, b) =>
                    a.F.CompareTo(b.F)),
                _ => throw new System.ArgumentException("Unsupported queuing function")
            };
        }

        public IEnumerator SearchCoroutine(SearchProblem searchProblem, MonoBehaviour owner)
        {
            _searchTimer = Stopwatch.StartNew();
            if (_queueingFunction is IDS)
            {
                yield return owner.StartCoroutine(RunIdsCoroutine(searchProblem));
            }
            else
            {
                yield return owner.StartCoroutine(RunSingleSearchCoroutine(searchProblem));
            }
            Debug.Log("Finished?");
            _searchTimer.Stop();
            _searchListener?.OnSearchComplete(_searchResult);
            _searchResult.PrintSummary();
        }

        private IEnumerator RunSingleSearchCoroutine(SearchProblem searchProblem, int? levelLimit = null)
        {
            Reset();
            var initialHeuristic = _queueingFunction.isInformed
                ? searchProblem.GetHeuristicCost(searchProblem.initialState)
                : 0f;
            var startNode = new SearchNode(searchProblem.initialState, 0, heuristicCost: initialHeuristic);
            _searchListener?.OnNodeGenerated(startNode);
            
            if (_levelManager.useGraphSearch)
            {
                _searchNodes[startNode.state] = startNode;
            }
            _frontier.Add(startNode);
            _searchListener?.OnNodesAddedToFrontier(new List<SearchNode> { startNode });

            while (!_frontier.IsEmpty)
            {
                var node = _frontier.Remove();
                if (_levelManager.useGraphSearch && _searchNodes[node.state] != node)
                    continue;

                if (searchProblem.IsGoal(node.state))
                {
                    var solutionPath = node.GetPathActions();
                    _searchResult = SearchResult.Found(node, _expanded.Count,
                        _expanded.Count + _frontier.Count, solutionPath: solutionPath,
                        level: levelLimit ?? _levelLimit, timeS: ElapsedTimeinS);
                    _searchListener?.OnSolutionFound(node);
                    yield break;
                }

                if (levelLimit.HasValue && node.depth >= levelLimit.Value)
                {
                    _searchListener?.OnNodeExpanded(node);
                    continue;
                }
                    

                var successors = Expand(node, searchProblem);
                foreach (var successor in successors)
                    _searchListener?.OnNodeGenerated(successor);

                _frontier.AddRange(successors);
                _searchListener?.OnNodesAddedToFrontier(_frontier.ToList());

                if (_expanded.Count > expansionLimit)
                {
                    _searchResult = SearchResult.Failed($"Step limit of {expansionLimit} exceeded. " +
                                                        $"No solution found", _expanded.Count,
                        _expanded.Count + _frontier.Count, level: levelLimit ?? _levelLimit,
                        timeS: ElapsedTimeinS);
                    yield break;
                }

                if(_frontier.Count != 0)
                    _searchListener?.OnNodeExpanding(_frontier.Peek());
                
                if (_levelManager.isStepped)
                {
                    _searchTimer.Stop();
                    _stepRequested = false;
                    yield return new WaitUntil(() => _stepRequested && !IsAnimating());
                    _searchTimer.Start();
                }
                else
                {
                    _searchTimer.Stop();
                    yield return null;
                    _searchTimer.Start();
                }
            }

            _searchResult = SearchResult.Failed("Exhausted state space. No solution found", _expanded.Count,
                _expanded.Count + _frontier.Count, level: levelLimit ?? _levelLimit, timeS: ElapsedTimeinS);
        }

        private bool IsAnimating() => _levelManager.ProblemVisualizer.IsAnimating;

        private List<SearchNode> Expand(SearchNode node, SearchProblem searchProblem)
        {
            var actions = searchProblem.actions;
            var transitionFunction = searchProblem.transitionFunction;
            var stepCostFunction = searchProblem.stepCostFunction;
            List<SearchNode> successors = new();

            foreach (var action in actions)
            {
                var successorState = transitionFunction.GetSuccessor(node.state, action);
                if (successorState == null || (node.parent != null && successorState.Equals(node.parent.state)))
                    continue;

                var candidateDepth = node.depth + 1;
                var stepCost = stepCostFunction.GetCost(node.state, action, successorState);
                var heuristic = _queueingFunction.isInformed
                    ? searchProblem.GetHeuristicCost(successorState)
                    : 0f;

                // Tree search: no need to check for existing nodes or expanded states. Keep duplicates in frontier.
                if (!_levelManager.useGraphSearch)
                {
                    var newNode = new SearchNode(successorState, candidateDepth, action, node, stepCost, heuristic);
                    successors.Add(newNode);
                    continue;
                }

                // Graph search: check for existing nodes and expanded states to avoid duplicates in frontier.
                if (!_searchNodes.TryGetValue(successorState, out var existingNode))
                {
                    var successor = new SearchNode(successorState, candidateDepth, action, node,
                        stepCost: stepCost,
                        heuristicCost: heuristic);
                    _searchNodes[successorState] = successor;
                    successors.Add(successor);
                    continue;
                }

                if (_queueingFunction is IDS && candidateDepth < existingNode.depth)
                {
                    existingNode.SetDepth(candidateDepth);
                    existingNode.SetParent(node, action, stepCost);
                    if (_frontier.Contains(existingNode))
                    {
                        if (_frontier is PriorityQueue<SearchNode> pq)
                        {
                            pq.TryUpdate(existingNode);
                            _searchListener.OnFrontierReordered();
                        }
                            
                    }
                    else
                    {
                        successors.Add(existingNode);
                    }

                    _expanded.Remove(existingNode);
                }
            }

            _expanded.Add(node);
            _searchListener?.OnNodeExpanded(node);
            return successors;
        }

        private IEnumerator RunIdsCoroutine(SearchProblem searchProblem)
        {
            for (var limit = 0; limit <= _levelLimit; limit++)
            {
                _levelManager.ProblemVisualizer.ClearNodeVisuals();
                yield return RunSingleSearchCoroutine(searchProblem, limit);

                if (_searchResult is { success: true })
                    yield break;
            }
        }

        private void Reset()
        {
            _frontier.Clear();
            _expanded.Clear();
            _searchResult = null;
            _searchNodes?.Clear();
        }

        /// <summary>Called by the UI to advance one step.</summary>
        public void AdvanceStep() => _stepRequested = true;
    }
}
