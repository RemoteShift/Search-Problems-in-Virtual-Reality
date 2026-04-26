using System.Collections.Generic;
using Search.Core;
using Search.Levels;
using Search.Visualization;

namespace Search.Controllers
{
    public class SearchController : Utils.Singleton<SearchController>, ISearchListener
    {
        private readonly IVisualizer _problemVisualizer = LevelManager.Instance.ProblemVisualizer;
        
        public void OnNodeExpanded(SearchNode node)
        {
            _problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Expanded);
        }

        public void OnNodeExpanding(SearchNode node)
        {
            _problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Expanding);
            _problemVisualizer.BlinkNode(node.state);
        }

        public void OnNodeGenerated(SearchNode node)
        {
            _problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Default);
        }

        public void OnFrontierReordered()
        {
            // TODO: Breh 0
        }

        public void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> nodes)
        {
            foreach (var node in nodes)
            {
                _problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Frontier);
            }
            
        }

        public void OnSolutionFound(SearchNode solution)
        {
            // TODO: Breh 2
        }

        public void OnSearchComplete(SearchResult result)
        {
            // TODO: Breh 3
        }

        public void AddEdge(SearchNode a, SearchNode b)
        {
            var nodeA = _problemVisualizer.GetOrCreateNodeVisual(a.state);
            var nodeB = _problemVisualizer.GetOrCreateNodeVisual(b.state);
            
            EdgeManager.Instance.AddEdge(a.state.id, b.state.id, nodeA.transform, nodeB.transform);
        }

        public void RemoveEdge(SearchNode a, SearchNode b)
        {
            EdgeManager.Instance.RemoveEdge(a.state.id, b.state.id);
        }

        public void AdvanceStep()
        {
            var search = LevelManager.Instance.searchAlgorithm;
            
            search?.AdvanceStep();
        }
    }
}

