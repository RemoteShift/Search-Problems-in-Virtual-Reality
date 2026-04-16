using System.Collections.Generic;

namespace Search.Core
{
    #region Summary

    /// <summary>
    /// Strategy that determines the order in which nodes are explored.
    /// Corresponds to the queuing function f in the General Search Algorithm.
    /// </summary>

    #endregion
    public interface IQueuingFunction
    {
        #region Summary

        /// <summary>
        /// Takes the current frontier and a list of newly generated successor nodes,
        /// and returns a new frontier (ordered according to the strategy).
        /// </summary>

        #endregion
        IFrontier<SearchNode> Reorder(IFrontier<SearchNode> frontier, IEnumerable<SearchNode> successors);

        bool isInformed { get; }
    }
}