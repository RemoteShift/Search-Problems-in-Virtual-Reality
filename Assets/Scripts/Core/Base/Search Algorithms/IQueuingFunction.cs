using System.Collections.Generic;

namespace Search.Core
{
    /// <summary>
    /// Strategy that determines the order in which nodes are explored.
    /// Corresponds to the queuing function f in the General Search Algorithm.
    /// </summary>
    public interface IQueuingFunction
    {
        /// <summary>
        /// Takes the current frontier and a list of newly generated successor nodes,
        /// and returns a new frontier (ordered according to the strategy).
        /// </summary>
        IEnumerable<SearchNode> Reorder(IEnumerable<SearchNode> frontier, IEnumerable<SearchNode> successors);
    }
}