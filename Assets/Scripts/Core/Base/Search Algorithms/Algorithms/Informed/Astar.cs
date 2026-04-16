using System.Collections.Generic;

namespace Search.Core.Algorithms
{
    public class Astar : IQueuingFunction
    {
        public IFrontier<SearchNode> Reorder(IFrontier<SearchNode> frontier, IEnumerable<SearchNode> successors)
        {
            if (frontier is not Utils.Queue<SearchNode>)
            {
                throw new System.InvalidOperationException("A* requires a priority-queue-based frontier.");
            }
            
            foreach (var node in successors)
            {
                frontier.Add(node);
            }

            return frontier;
        }

        public bool isInformed => true;
    }
}
