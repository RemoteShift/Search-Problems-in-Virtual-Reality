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
        private IVisualizer problemVisualizer => LevelManager.Instance?.ProblemVisualizer;
        private TreeVisualizer treeVisualizer => LevelManager.Instance.visualizeTree ? 
            LevelManager.Instance?.TreeVisualizer : null;

        public void OnNodeExpanded(SearchNode node)
        {
            problemVisualizer?.GetOrCreateNodeVisual(node).SetState(NodeState.Expanded);
            treeVisualizer?.GetOrCreateNodeVisual(node).SetState(NodeState.Expanded);
        }

        public void OnNodeExpanding(SearchNode node)
        {
            problemVisualizer?.GetOrCreateNodeVisual(node).SetState(NodeState.Expanding);
            problemVisualizer?.BlinkNode(node.state, Color.red);
            problemVisualizer?.TryPlaySameState(node);
            
            treeVisualizer?.GetOrCreateNodeVisual(node).SetState(NodeState.Expanding);
            treeVisualizer?.BlinkNode(node, Color.red);
            treeVisualizer?.TryPlaySameState(node);
        }

        public void OnNodeGenerated(SearchNode node)
        {
            problemVisualizer?.GetOrCreateNodeVisual(node).SetState(NodeState.Default);
            treeVisualizer?.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Default);
        }

        public void OnFrontierReordered()
        {
            // TODO: Breh 0
        }

        public void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> nodes)
        {
            foreach (var node in nodes)
            {
                problemVisualizer?.GetOrCreateNodeVisual(node).SetState(NodeState.Frontier);
                treeVisualizer?.GetOrCreateNodeVisual(node, node.parent).SetState(NodeState.Frontier);
            }
        }

        public void OnSolutionFound(SearchNode solution)
        {
            // TODO: Breh 2
        }

        public void OnSearchComplete(SearchResult result)
        {
            if (result.success)
            {
                var node = result.solutionNode;

                while (node != null)
                {
                    problemVisualizer?.GetNodeVisual(node).SetState(NodeState.Path);
                    treeVisualizer?.GetNodeVisual(node).SetState(NodeState.Path);
                    node = node.parent;
                }
            }
            
            result.PrintSummary();
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
            
            var nodeAProblem = problemVisualizer?.GetOrCreateNodeVisual(a);
            var nodeBProblem = problemVisualizer?.GetOrCreateNodeVisual(b);
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

        public void ResetParent(SearchNode childNode)
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
        }

        public void AdvanceStep()
        {
            var search = LevelManager.Instance.searchAlgorithm;

            search?.AdvanceStep();
        }
    }
}