using System.Collections.Generic;
using System.Linq;
using Search.Levels;
using Search.Utils;

namespace Search.Core.Algorithms
{
    public class GeneralSearch
    {
        private readonly IQueuingFunction _queueingFunction;
        
        private readonly IFrontier<SearchNode> _frontier;

        private int _levelLimit;
        
        private LevelManager _levelManager;
        
        public GeneralSearch(SearchProblem searchProblem, IQueuingFunction queueingFunction, int levelLimit = 0)
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
            var initialHeuristic = _queueingFunction.isInformed
                ? searchProblem.GetHeuristicCost(searchProblem.initialState)
                : 0f;
            var startNode = new SearchNode(searchProblem.initialState, heuristicCost: initialHeuristic);

            _frontier.Add(startNode);

            while (!_frontier.IsEmpty)
            {
                var node = _frontier.Remove();

                if (searchProblem.IsGoal(node.state))
                {
                    // Goal found, return the solution path
                    var solutionPath = node.GetPathActions();
                    //SearchResult searchResult = SearchResult.Found(node, );
                    // Handle the solution path as needed (e.g., print it, return it, etc.)
                    return;
                }

                var successors = Expand(node, searchProblem);
                _frontier.AddRange(successors);

                /*if (_levelLimit == 0 || node.Depth < _levelLimit)
                {
                    var successors = searchProblem.GetSuccessors(node);
                    _queueingFunction.Reorder(_frontier, successors);
                }*/
            }

            //var searchResult = SearchResult.Failed("Allahu a3lam", )
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
                var successor = new SearchNode(successorState, action, node,
                    stepCostFunction.GetCost(node.state, action, successorState),
                    _queueingFunction.isInformed ? searchProblem.GetHeuristicCost(successorState) : 0f);
                successors.Add(successor);
                _levelManager.mazeVisualizer.OnNodeGenerated(successor);
            }
            
            _levelManager.mazeVisualizer.OnNodeExpanded(node);
            return successors;
        }
    }
}
