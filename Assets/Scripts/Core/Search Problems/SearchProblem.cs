using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SearchCore
{
    public class SearchProblem
    {
        public List<SearchNode> stateSpace { get; private set; }
    
        public List<string> actions { get; private set; }
    
        public SearchNode initialState { get; private set; }
    
        private readonly Func<SearchNode, bool> _goalTest;
    
        public Dictionary<SearchNode, Dictionary<string, Successor>> transitions { get; private set; }

        public struct Successor {
            public SearchNode NextState;
            public float Cost;
        }

        public bool TestGoal(SearchNode searchNode) => _goalTest(searchNode);

        public SearchProblem(SearchNode initialState, Func<SearchNode, bool> goalTest, List<string> actions,
            [CanBeNull] List<SearchNode> stateSpace = null)
        {
            this.initialState = initialState;
            _goalTest = goalTest;
            this.actions = actions;
            transitions = new Dictionary<SearchNode, Dictionary<string, Successor>>();
            this.stateSpace = stateSpace ?? new List<SearchNode>();
        }
    }   
}