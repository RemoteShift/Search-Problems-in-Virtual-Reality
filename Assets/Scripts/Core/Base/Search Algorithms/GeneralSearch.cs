using System;
using System.Collections.Generic;
using System.Linq;
using Search.Levels;
using Search.Utils;
using UnityEngine;

namespace Search.Core.Algorithms
{
    public class GeneralSearch
    {
        private readonly IQueuingFunction _queueingFunction;
        
        private readonly IFrontier<SearchNode> _frontier;
        private readonly List<SearchNode> _expanded = new();
        private readonly Dictionary<IState, SearchNode> _searchNodes = new();
        
        private int? _levelLimit;
        
        private LevelManager _levelManager;
        
        private SearchResult _searchResult;
        
        public GeneralSearch(IQueuingFunction queueingFunction, int? levelLimit = 0)
        {
            _queueingFunction = queueingFunction;
            _levelLimit = levelLimit;
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

        public void Search(SearchProblem searchProblem)
        {
            if (_queueingFunction is IDS)
            {
                RunIDS(searchProblem);
                _searchResult.PrintSummary();
                return;
            }
            
            RunSingleSearch(searchProblem);
            _searchResult.PrintSummary();
        }

        private void RunSingleSearch(SearchProblem searchProblem, int? levelLimit = null)
        {
            Reset();
            var initialHeuristic = _queueingFunction.isInformed
                ? searchProblem.GetHeuristicCost(searchProblem.initialState)
                : 0f;
            var startNode = new SearchNode(searchProblem.initialState, 0, heuristicCost: initialHeuristic);

            _searchNodes[startNode.state] = startNode;
            _frontier.Add(startNode);

            while (!_frontier.IsEmpty)
            {
                var node = _frontier.Remove();
                if (_searchNodes[node.state] != node)
                    continue;

                if (searchProblem.IsGoal(node.state))
                {
                    var solutionPath = node.GetPathActions();
                    _searchResult = SearchResult.Found(node, _expanded.Count,
                        _expanded.Count + _frontier.Count, solutionPath: solutionPath, 
                        level: levelLimit ?? _levelLimit);
                    return;
                }

                if (levelLimit.HasValue && node.depth >= levelLimit.Value)
                    continue;
                
                var successors = Expand(node, searchProblem);
                _frontier.AddRange(successors);
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

                if (!_searchNodes.TryGetValue(successorState, out var existingNode))
                {
                    var successor = new SearchNode(successorState, candidateDepth, action, node,
                        stepCost: stepCost,
                        _queueingFunction.isInformed ? searchProblem.GetHeuristicCost(successorState) : 0f);
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
                    continue;
                }
            }
            
            _expanded.Add(node);
            //_levelManager.mazeVisualizer.OnNodeExpanded(node);
            return successors;
        }
        
        private void RunIDS(SearchProblem searchProblem)
        {
            for (var limit = 0; limit <= _levelLimit ; limit++)
            {
                RunSingleSearch(searchProblem, limit);
                
                if (_searchResult is { success: true })
                    return;
            }
        }
        
        private void Reset()
        {
            _frontier.Clear();
            _expanded.Clear();
            _searchNodes.Clear();
            _searchResult = null;
        }
    }
}
