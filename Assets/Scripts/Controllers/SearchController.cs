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
        private readonly IVisualizer _problemVisualizer = LevelManager.Instance.ProblemVisualizer;
        private readonly TreeVisualizer _treeVisualizer = LevelManager.Instance.TreeVisualizer;

        public void OnNodeExpanded(SearchNode node)
        {
            _problemVisualizer.GetOrCreateNodeVisual(node).SetState(NodeState.Expanded);
            _treeVisualizer.GetOrCreateNodeVisual(node).SetState(NodeState.Expanded);
        }

        public void OnNodeExpanding(SearchNode node)
        {
            _problemVisualizer.GetOrCreateNodeVisual(node).SetState(NodeState.Expanding);
            _problemVisualizer.BlinkNode(node.state, Color.red);
            _problemVisualizer.TryPlaySameState(node);
            
            _treeVisualizer.GetOrCreateNodeVisual(node).SetState(NodeState.Expanding);
            _treeVisualizer.BlinkNode(node, Color.red);
            _treeVisualizer.TryPlaySameState(node);
        }

        public void OnNodeGenerated(SearchNode node)
        {
            _problemVisualizer.GetOrCreateNodeVisual(node).SetState(NodeState.Default);
            _treeVisualizer.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Default);
        }

        public void OnFrontierReordered()
        {
            // TODO: Breh 0
        }

        public void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> nodes)
        {
            foreach (var node in nodes)
            {
                _problemVisualizer.GetOrCreateNodeVisual(node).SetState(NodeState.Frontier);
                _treeVisualizer.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Frontier);
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
            var nodeAProblem = _problemVisualizer.GetOrCreateNodeVisual(a);
            var nodeBProblem = _problemVisualizer.GetOrCreateNodeVisual(b);
            EdgeManager.Instance.AddEdge(a.state.id, b.state.id, nodeAProblem.transform, nodeBProblem.transform);

            if (LevelManager.Instance.useGraphSearch && _treeVisualizer.GetNodeVisual(b))
            {
                return;
            }
            var nodeATree = _treeVisualizer.GetOrCreateNodeVisual(a, a.parent);
            var nodeBTree = _treeVisualizer.GetOrCreateNodeVisual(b, b.parent);
            
            var uniqueIdA = a.GetHashCode().ToString();
            var uniqueIdB = b.GetHashCode().ToString();

            EdgeManager.Instance.AddEdge(uniqueIdA, uniqueIdB, nodeATree.transform, nodeBTree.transform);
        }

        public void RemoveEdge(SearchNode a, SearchNode b)
        {
            EdgeManager.Instance.RemoveEdge(a.state.id, b.state.id);
            EdgeManager.Instance.RemoveEdge(a.state.id + "_tree", b.state.id + "_tree");
        }

        public void AdvanceStep()
        {
            var search = LevelManager.Instance.searchAlgorithm;

            search?.AdvanceStep();
        }
    }
}