namespace SearchCore
{
    public class MazeStep : IStepCostFunction
    {
        public float GetCost(IState fromState, string action, IState toState)
        {
            return 1;
        }
    }
}
