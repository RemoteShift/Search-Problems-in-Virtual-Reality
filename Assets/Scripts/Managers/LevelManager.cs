using UnityEngine;
using Search.Core;
using Search.Utils;
using Search.Visualization;

namespace Search.Levels
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Level Asset")] [SerializeField]
        private LevelData currentLevel;

        [Header("Scene References")]
        public MazeVisualizer mazeVisualizer;

        // For graph levels you could add GraphVisualizer later
        //[SerializeField] private VRSearchController searchController;

        private SearchProblem _currentProblem;

        private void Start()
        {
            if (currentLevel)
                LoadLevel(currentLevel);
            else
                Debug.LogWarning("No level assigned to LevelManager");
        }

        private void LoadLevel(LevelData level)
        {
            currentLevel = level;
            _currentProblem = level.CreateSearchProblem();
            
            if (mazeVisualizer && level is MazeLevelData mazeLevel)
            {
                mazeVisualizer.Setup(mazeLevel, _currentProblem);
            }
            // Add future support for GraphLevelData here:
            // else if (level is GraphLevelData graphLevel && graphVisualizer != null)
            //     graphVisualizer.Setup(graphLevel, _currentProblem);

            // Notify search controller
            //if (searchController != null)
                //searchController.SetProblem(_currentProblem);
        }

        public SearchProblem GetCurrentProblem() => _currentProblem;
        public LevelData GetCurrentLevel() => currentLevel;
    }
}