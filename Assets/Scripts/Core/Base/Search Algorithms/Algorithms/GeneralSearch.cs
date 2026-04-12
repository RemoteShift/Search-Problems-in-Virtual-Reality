namespace Search.Core
{
    public class GeneralSearch
    {
        private readonly IQueuingFunction _queueingFunction;
        
        public GeneralSearch(IQueuingFunction queueingFunction)
        {
            _queueingFunction = queueingFunction;
        }
    }
}
