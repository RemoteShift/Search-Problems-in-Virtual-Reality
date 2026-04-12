namespace Search.Core
{
    public interface ITransitionFunction
    {
        IState GetSuccessor(IState state, string action);
    }
}