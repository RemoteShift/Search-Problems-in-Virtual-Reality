using Search.Utils;

namespace Search.Core.Algorithms
{
    public class GeneralSearch
    {
        private readonly IQueuingFunction _queueingFunction;
        
        private readonly IFrontier<SearchNode> _frontier;
        
        public GeneralSearch(SearchProblem searchProblem, IQueuingFunction queueingFunction)
        {
            _queueingFunction = queueingFunction;
            if (_queueingFunction is BFS or DFS)
            {
                _frontier = new Queue<SearchNode>();
            }
            else
            {
                //_frontier = new PriorityQueue<SearchNode>((a,b) => a.PathCost.CompareTo(b.PathCost));
            }
        }
    }
}
