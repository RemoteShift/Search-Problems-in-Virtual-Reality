using System;
using System.Collections.Generic;

namespace SearchCore
{
    public class SearchProblem
    {
        public IReadOnlyList<string> actions { get; private set; }
    
        public IState initialState { get; private set; }
    
        private readonly Func<IState, bool> _goalTest;
    
        public ITransitionFunction transitionFunction { get; }
        public IStepCostFunction stepCostFunction { get; }
        
        public SearchProblem(
            IState initialState,
            Func<IState, bool> goalTest,
            IReadOnlyList<string> actions,
            ITransitionFunction transitionFunction,
            IStepCostFunction stepCostFunction)
        {
            this.initialState = initialState;
            this._goalTest = goalTest;
            this.actions = actions;
            this.transitionFunction = transitionFunction;
            this.stepCostFunction = stepCostFunction;
        }
    }
}