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
        void OnFrontierReordered();
        
        /// <summary>
        /// Called when a node is added to the frontier.
        /// </summary>
        /// <param name="node">The new node</param>
        void OnNodesAddedToFrontier(IReadOnlyList<SearchNode> node);

        /// <summary>
        /// Called when a solution is found.
        /// </summary>
        void OnSolutionFound(SearchNode solution);

        /// <summary>
        /// Called when the search completes.
        /// </summary>
        void OnSearchComplete(SearchResult result);
    }
}
