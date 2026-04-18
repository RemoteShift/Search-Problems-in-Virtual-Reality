using System.Collections.Generic;

namespace Search.Core
{
    public class SearchResult
    {
        public bool success { get; }
        private SearchNode solutionNode { get; }
        private List<string> solutionPath { get; }
        public int nodesExpanded { get; }
        private int nodesGenerated { get; }
        private int totalTimeMs { get; }
        private string failureReason { get; }

        private SearchResult(bool success,
            int nodesExpanded,
            int nodesGenerated,
            int totalTimeMs,
            string failureReason = null,
            SearchNode solutionNode = null,
            List<string> solutionPath = null)
        {
            this.success = success;
            this.solutionNode = solutionNode;
            this.solutionPath = solutionPath;
            this.nodesExpanded = nodesExpanded;
            this.nodesGenerated = nodesGenerated;
            this.totalTimeMs = totalTimeMs;
            this.failureReason = failureReason;
        }

        public static SearchResult Found(SearchNode node, int expanded, int generated, List<string> solutionPath,
            int timeMs = 0)
            => new SearchResult(true, expanded, generated, timeMs, solutionNode: node, solutionPath: solutionPath);

        public static SearchResult Failed(string reason, int expanded, int generated, int timeMs)
            => new SearchResult(false, expanded, generated, timeMs, failureReason: reason);

        public void PrintSummary()
        {
            UnityEngine.Debug.Log(success
                ? $"Search successful! Solution Node: {solutionNode.state.id}\n" +
                  $"Solution path: {string.Join(" -> ", solutionPath)}"
                : $"Search failed. Reason: {failureReason}");
            UnityEngine.Debug.Log(
                $"Nodes Expanded: {nodesExpanded}, Nodes Generated: {nodesGenerated}, Time: {totalTimeMs} ms");
        }
    }
}
