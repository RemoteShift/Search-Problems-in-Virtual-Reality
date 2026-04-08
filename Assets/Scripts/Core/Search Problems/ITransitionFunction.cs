namespace SearchCore
{
    public interface ITransitionFunction
    {
        IState GetSuccessor(IState state, string action);
    }
}