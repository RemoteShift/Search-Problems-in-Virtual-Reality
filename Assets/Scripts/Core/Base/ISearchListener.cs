using System.Collections.Generic;

namespace Search.Core
{
    public interface ISearchListener
    {
        /// <summary>
        /// Called when a node is expanded.
        /// </summary>
        void OnNodeExpanded(SearchNode node);
        
        /// <summary>
        /// Called when a node is about to be expanded in the next step.
        /// </summary>
        void OnNodeExpanding(SearchNode node);

        /// <summary>
        /// Called when a node is generated.
        /// </summary>
        void OnNodeGenerated(SearchNode node);

        /// <summary>
        /// Called after the frontier has been reordered.
        /// Provides the current frontier for visualization.
        /// </summary>
        void OnFrontierReordered(IReadOnlyList<SearchNode> frontier);
        
        /// <summary>
        /// Called when a node is added to the frontier.
        /// </summary>
        /// <param name="node">The new node</param>
        void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> node);

        /// <summary>
        /// Called when the search completes.
        /// </summary>
        void OnSearchComplete(SearchResult result);
        
        /// <summary>
        /// Adds an edge between nodes a and b in the visualizer(s)
        /// </summary>
        void AddEdge(SearchNode a, SearchNode b);
        
        /// <summary>
        /// Removes the edge between the nodes a and b in the visualizer(s)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void RemoveEdge(SearchNode a, SearchNode b);

        /// <summary>
        /// Resets the parent of the given child node in the visualizer(s) to match the current parent in the search
        /// tree.
        /// </summary>
        /// <remarks>
        /// Make sure you have already updated the child node's parent in the search tree before calling this method,
        /// as it will read the current parent from the child node and update the visualizer(s) accordingly.
        /// </remarks>
        /// <param name="childNode"></param>
        void ResetParent(SearchNode childNode, IReadOnlyList<SearchNode> frontier);

        bool isAutomaticSearch { get; }
    }
}
