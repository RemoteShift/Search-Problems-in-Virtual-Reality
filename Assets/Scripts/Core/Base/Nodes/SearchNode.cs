using System.Collections.Generic;

namespace Search.Core
{
    public class SearchNode
    {
        public IState state { get; }
        public SearchNode parent { get; }
        public string actionFromParent { get; }
        public float pathCost { get;  } // g(n)
        public int depth { get; }
        public SearchNode(IState state,  
            string actionFromParent = null, 
            SearchNode parent = null, 
            float stepCost = 0)
        {
            this.state = state;
            this.parent = parent;
            this.actionFromParent = actionFromParent;
            this.pathCost = stepCost + (parent?.pathCost ?? 0);
            this.depth = parent?.depth + 1 ?? 0;
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
    }
}
