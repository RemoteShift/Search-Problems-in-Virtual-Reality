using UnityEngine;
using Search.Core;

namespace Search.Levels
{
    public abstract class LevelData : ScriptableObject
    {
        public string levelName;
        public string description;
        public int expansionLimit;

        public abstract SearchProblem CreateSearchProblem();
    }
}
