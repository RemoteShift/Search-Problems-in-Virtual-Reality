using System.Collections.Generic;

namespace Search.Core.Algorithms
{
    public class BFS : IQueuingFunction
    {
        public IFrontier<SearchNode> Reorder(IFrontier<SearchNode> frontier, IEnumerable<SearchNode> successors)
        {
            if (frontier is not Utils.Queue<SearchNode>)
            {
                throw new System.InvalidOperationException("BFS requires a queue-based frontier.");
            }
            
            foreach (var node in successors)
            {
                frontier.Add(node);
            }

            return frontier;
        }
        
        public bool isInformed => false;
    }
}
