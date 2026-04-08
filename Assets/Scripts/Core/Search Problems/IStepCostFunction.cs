namespace SearchCore
{
    public interface IStepCostFunction
    {
        float GetCost(IState fromState, string action, IState toState);
    }
}