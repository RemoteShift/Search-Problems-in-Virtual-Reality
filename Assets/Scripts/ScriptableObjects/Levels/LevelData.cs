using UnityEngine;
using SearchCore;

namespace SearchLevels
{
    public abstract class LevelData : ScriptableObject
    {
        public string levelName;
        public string description;

        public abstract SearchProblem CreateSearchProblem();
    }
}
