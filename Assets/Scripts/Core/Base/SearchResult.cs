using System.Collections.Generic;

namespace Search.Core
{
    public class SearchResult
    {
        public bool success { get; }
        public SearchNode solutionNode { get; }
        public List<string> solutionPath { get; }
        public int nodesExpanded { get; }
        private int nodesGenerated { get; }
        private int? level { get; }
        private float totalTimeS { get; }
        private string failureReason { get; }

        private SearchResult(bool success,
            int nodesExpanded,
            int nodesGenerated,
            float totalTimeS,
            string failureReason = null,
            SearchNode solutionNode = null,
            List<string> solutionPath = null,
            int? level = null)
        {
            this.success = success;
            this.solutionNode = solutionNode;
            this.solutionPath = solutionPath;
            this.nodesExpanded = nodesExpanded;
            this.nodesGenerated = nodesGenerated;
            this.totalTimeS = totalTimeS;
            this.failureReason = failureReason;
            this.level = level;
        }

        public static SearchResult Found(SearchNode node, int expanded, int generated, List<string> solutionPath,
            float timeS = 0f, int? level = null)
            => new SearchResult(true, expanded, generated, timeS, solutionNode: node, 
                solutionPath: solutionPath, level: level);

        public static SearchResult Failed(string reason, int expanded, int generated, float timeS = 0f, 
            int? level = null)
            => new SearchResult(false, expanded, generated, timeS, failureReason: reason, 
                level: level);

        public void PrintSummary()
        {
            UnityEngine.Debug.Log(success
                ? $"Search successful! Solution Node: {solutionNode.state.id}\n" +
                  $"Solution path: {string.Join(" -> ", solutionPath)}"
                : $"Search failed. Reason: {failureReason}");
            UnityEngine.Debug.Log( level.HasValue ? 
                $"Depth {level}: Nodes Expanded: {nodesExpanded}, Nodes Generated: {nodesGenerated}, " +
                $"Time: {totalTimeS} Seconds" :
                $"Nodes Expanded: {nodesExpanded}, Nodes Generated: {nodesGenerated}, Time: {totalTimeS} Seconds");
        }
    }
}
