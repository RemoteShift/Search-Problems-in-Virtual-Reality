using System.Collections.Generic;

namespace Search.Core.Algorithms
{
    public class IDS : IQueuingFunction
    {
        public IFrontier<SearchNode> Reorder(IFrontier<SearchNode> frontier, IEnumerable<SearchNode> successors)
        {
            if (frontier is not Utils.Stack<SearchNode>)
            {
                throw new System.InvalidOperationException("IDS requires a stack-based frontier.");
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
