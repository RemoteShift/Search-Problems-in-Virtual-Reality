using System.Collections.Generic;
using UnityEngine;
using Search.Core;

namespace Search.Levels
{
    public abstract class LevelData : ScriptableObject
    {
        public string levelName;
        public string description;
        public int expansionLimit;

        public abstract IState startState { get; set; }
        public abstract List<IState> goalStates { get; set; }
        
        public abstract SearchProblem CreateSearchProblem();
    }
}
