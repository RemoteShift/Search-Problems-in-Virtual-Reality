namespace SearchCore
{
    public class SearchResult
    {
        public bool success { get; }
        public SearchNode solutionNode { get; }
        public int nodesExpanded { get; }
        public int nodesGenerated { get; }
        public int totalTimeMs { get; }
        public string failureReason { get; }

        public SearchResult(bool success, 
            SearchNode solutionNode, 
            int nodesExpanded, 
            int nodesGenerated, 
            int totalTimeMs, 
            string failureReason)
        {
            this.success = success;
            this.solutionNode = solutionNode;
            this.nodesExpanded = nodesExpanded;
            this.nodesGenerated = nodesGenerated;
            this.totalTimeMs = totalTimeMs;
            this.failureReason = failureReason;
        }
        
        public static SearchResult Found(SearchNode node, int expanded, int generated, int timeMs)
            => new SearchResult(true, node, expanded, generated, timeMs, null);

        public static SearchResult Failed(string reason, int expanded, int generated, int timeMs)
            => new SearchResult(false, null, expanded, generated, timeMs, reason);
    }
}
