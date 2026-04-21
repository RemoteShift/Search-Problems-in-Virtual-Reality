using System.Collections;
using System.Collections.Generic;
using Search.Levels;
using Search.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Search.Core.Algorithms
{
    [System.Serializable]
    public class GeneralSearch
    {
        private readonly IQueuingFunction _queueingFunction;

        [FormerlySerializedAs("_useGraphSearch")]
        [Tooltip("Whether to use graph search (track explored states and avoid duplicates in frontier) " +
                 "or tree search (allow duplicates in frontier).")]
        public bool useGraphSearch;
        
        private readonly IFrontier<SearchNode> _frontier;
        private readonly List<SearchNode> _expanded = new();
        private readonly Dictionary<IState, SearchNode> _searchNodes;
        
        private readonly int? _levelLimit;
        
        private LevelManager _levelManager;
        
        private SearchResult _searchResult;

        [Tooltip("Maximum number of nodes to expand before terminating search with failure.")]
        public int expansionLimit;
        
        public GeneralSearch(IQueuingFunction queueingFunction, int? levelLimit = 0, bool useGraphSearch = false,
            int expansionLimit = 10000)
        {
            _queueingFunction = queueingFunction;
            _searchNodes = useGraphSearch ? new() : null;
            this.useGraphSearch = useGraphSearch;
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
            _levelManager = LevelManager.Instance;
        }

        public IEnumerator SearchCoroutine(SearchProblem searchProblem, MonoBehaviour owner)
        {
            if (_queueingFunction is IDS)
            {
                yield return owner.StartCoroutine(RunIDSCoroutine(searchProblem));
            }
            else
            {
                yield return owner.StartCoroutine(RunSingleSearchCoroutine(searchProblem));
            }
            
            _searchResult.PrintSummary();
        }

        private IEnumerator RunSingleSearchCoroutine(SearchProblem searchProblem, int? levelLimit = null)
        {
            Reset();
            var initialHeuristic = _queueingFunction.isInformed
                ? searchProblem.GetHeuristicCost(searchProblem.initialState)
                : 0f;
            var startNode = new SearchNode(searchProblem.initialState, 0, heuristicCost: initialHeuristic);

            if (useGraphSearch)
            {
                _searchNodes[startNode.state] = startNode;
            }
            _frontier.Add(startNode);

            while (!_frontier.IsEmpty)
            {
                var node = _frontier.Remove();
                if (useGraphSearch && _searchNodes[node.state] != node)
                    continue;

                if (searchProblem.IsGoal(node.state))
                {
                    var solutionPath = node.GetPathActions();
                    _searchResult = SearchResult.Found(node, _expanded.Count,
                        _expanded.Count + _frontier.Count, solutionPath: solutionPath, 
                        level: levelLimit ?? _levelLimit);
                    yield break;
                }

                if (levelLimit.HasValue && node.depth >= levelLimit.Value)
                    continue;
                
                var successors = Expand(node, searchProblem);
                _frontier.AddRange(successors);

                if (_expanded.Count > expansionLimit)
                {
                    _searchResult = SearchResult.Failed($"Step limit of {expansionLimit} exceeded. " +
                                                        $"No solution found", _expanded.Count,
                        _expanded.Count + _frontier.Count, level: levelLimit ?? _levelLimit);
                    yield break;
                }
                
                yield return null;
            }

            _searchResult = SearchResult.Failed("Exhausted state space. No solution found", _expanded.Count, 
                _expanded.Count + _frontier.Count, level: levelLimit ?? _levelLimit);
        }

        private List<SearchNode> Expand(SearchNode node, SearchProblem searchProblem)
        {
            var actions = searchProblem.actions;
            var transitionFunction = searchProblem.transitionFunction;
            var stepCostFunction = searchProblem.stepCostFunction;
            List<SearchNode> successors = new();
            
            foreach (var action in actions)
            {
                var successorState = transitionFunction.GetSuccessor(node.state, action);
                if (successorState == null)
                    continue;
                
                var candidateDepth = node.depth + 1;
                var stepCost = stepCostFunction.GetCost(node.state, action, successorState);
                var heuristic = _queueingFunction.isInformed 
                    ? searchProblem.GetHeuristicCost(successorState) 
                    : 0f;
                
                // Tree search: no need to check for existing nodes or expanded states. Keep duplicates in frontier.
                if (!useGraphSearch)
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
                            pq.TryUpdate(existingNode);
                    }
                    else
                    {
                        successors.Add(existingNode);
                    }
                        
                    _expanded.Remove(existingNode);
                }
            }
            
            _expanded.Add(node);
            //_levelManager.mazeVisualizer.OnNodeExpanded(node);
            return successors;
        }
        
        private IEnumerator RunIDSCoroutine(SearchProblem searchProblem)
        {
            for (var limit = 0; limit <= _levelLimit ; limit++)
            {
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
    }
}
