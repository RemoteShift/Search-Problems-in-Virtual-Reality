using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Search.Core
{
    public class SearchProblem
    {
        public IReadOnlyList<string> actions { get; private set; }
    
        public IState initialState { get; private set; }
    
        private readonly Func<IState, bool> _goalTest;
        private readonly Func<IState, float> _heuristicFunction;
    
        public ITransitionFunction transitionFunction { get; }
        public IStepCostFunction stepCostFunction { get; }
        
        public SearchProblem(
            IState initialState,
            Func<IState, bool> goalTest,
            IReadOnlyList<string> actions,
            ITransitionFunction transitionFunction,
            IStepCostFunction stepCostFunction,
            [CanBeNull] Func<IState, float> heuristicFunction = null)
        {
            this.initialState = initialState;
            this._goalTest = goalTest;
            this._heuristicFunction = heuristicFunction ?? (_ => 0f);
            this.actions = actions;
            this.transitionFunction = transitionFunction;
            this.stepCostFunction = stepCostFunction;
        }
        
        public float GetHeuristicCost(IState state) => _heuristicFunction(state);
        
        public bool IsGoal(IState state) => _goalTest(state);
    }
}