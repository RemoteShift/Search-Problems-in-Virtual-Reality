using System.Collections.Generic;
using Search.Core;
using Search.Levels;
using Search.Utils;
using Search.Visualization;
using UnityEngine;

namespace Search.Controllers
{
    public class SearchController : Singleton<SearchController>, ISearchListener
    {
        public void OnNodeExpanded(SearchNode node)
        {
            var problemVisualizer = LevelManager.Instance.ProblemVisualizer;
            
            problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Expanded);
        }

        public void OnNodeExpanding(SearchNode node)
        {
            var problemVisualizer = LevelManager.Instance.ProblemVisualizer;

            
            problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Expanding);
            problemVisualizer.BlinkNode(node.state);
        }

        public void OnNodeGenerated(SearchNode node)
        {
            var problemVisualizer = LevelManager.Instance.ProblemVisualizer;

            problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Default);
        }

        public void OnFrontierReordered()
        {
            // TODO: Breh 0
        }

        public void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> nodes)
        {
            var problemVisualizer = LevelManager.Instance.ProblemVisualizer;

            foreach (var node in nodes)
            {
                problemVisualizer.GetOrCreateNodeVisual(node.state).SetState(NodeState.Frontier);
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

        public void AdvanceStep()
        {
            var search = LevelManager.Instance.searchAlgorithm;
            
            search?.AdvanceStep();
        }
    }
}

