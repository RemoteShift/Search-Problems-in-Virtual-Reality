using System.Collections.Generic;

namespace SearchCore
{
    public interface ISearchListener
    {
        /// <summary>
        /// Called when a node is expanded (removed from frontier and processed).
        /// </summary>
        void OnNodeExpanded(SearchNode node);

        /// <summary>
        /// Called when a node is generated (added to the frontier for the first time).
        /// </summary>
        void OnNodeGenerated(SearchNode node);

        /// <summary>
        /// Called after the frontier has been reordered (e.g., after each expansion).
        /// Provides the current frontier for visualization.
        /// </summary>
        void OnFrontierUpdated(IReadOnlyList<SearchNode> frontier);

        /// <summary>
        /// Called when a solution is found (before the search result is finalized).
        /// </summary>
        void OnSolutionFound(SearchNode solution);

        /// <summary>
        /// Called when the search completes (either found a solution or failed).
        /// </summary>
        void OnSearchComplete(SearchResult result);
    }
}
