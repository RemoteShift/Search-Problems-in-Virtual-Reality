using System.Collections.Generic;

namespace Search.Core
{
    public class SearchNode
    {
        public IState state { get; }
        public SearchNode parent { get; private set; }
        public string actionFromParent { get; private set; }
        public float pathCost { get; private set; } // g(n)
        public float heuristicCost { get; } // h(n)
        public float F => pathCost + heuristicCost; // f(n) = g(n) + h(n)

        public int depth { get; private set; }
        public SearchNode(IState state,  
            int depth,
            string actionFromParent = null, 
            SearchNode parent = null, 
            float stepCost = 0,
            float heuristicCost = 0)
        {
            this.state = state;
            this.parent = parent;
            this.actionFromParent = actionFromParent;
            this.pathCost = stepCost + (parent?.pathCost ?? 0);
            this.heuristicCost = heuristicCost;
            this.depth = depth;
        }
        
        public List<string> GetPathActions()
        {
            var actions = new List<string>();
            var currentNode = this;
            while (currentNode.parent != null)
            {
                actions.Add(currentNode.actionFromParent);
                currentNode = currentNode.parent;
            }
            actions.Reverse();
            return actions;
        }
        
        public void SetDepth(int newDepth)
        {
            depth = newDepth;
        }
        
        public void SetParent(SearchNode newParent, string actionFromNewParent, float stepCost)
        {
            parent = newParent;
            actionFromParent = actionFromNewParent;
            pathCost = stepCost + (newParent?.pathCost ?? 0);
        }
    }
}
