namespace Search.Core
{
    public interface IStepCostFunction
    {
        float GetCost(IState fromState, string action, IState toState);
    }
}