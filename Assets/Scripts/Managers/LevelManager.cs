using UnityEngine;
using Search.Core;
using Search.Core.Algorithms;
using Search.Utils;
using Search.Visualization;

namespace Search.Levels
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Level Asset")] [SerializeField]
        private LevelData currentLevel;

        [Header("Search Algorithm Settings")]
        [SerializeField] private AlgorithmType currentAlgorithmType = AlgorithmType.None;
        [SerializeField] private int levelLimit = 5;
        
        [Header("Scene References")]
        public MazeVisualizer mazeVisualizer;

        // For graph levels you could add GraphVisualizer later
        //[SerializeField] private VRSearchController searchController;

        private SearchProblem _currentProblem;
        private GeneralSearch _currentSearchAlgorithm;

        public void Start()
        {
            if (currentLevel)
                LoadLevel(currentLevel);
            else
                Debug.LogWarning("No level assigned to LevelManager");
            
            if (currentAlgorithmType != AlgorithmType.None)
                if(currentAlgorithmType == AlgorithmType.IDS)
                    StartSearch(levelLimit);
                else
                    StartSearch();
            else
                Debug.LogWarning("No search algorithm assigned to LevelManager");
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

        private void StartSearch(int? _levelLimit = null)
        {
            if (_currentProblem == null)
            {
                Debug.LogError("No search problem loaded. Cannot start search.");
                return;
            }

            IQueuingFunction queuingFunction = currentAlgorithmType switch
            {
                AlgorithmType.BFS => new BFS(),
                AlgorithmType.DFS => new DFS(),
                AlgorithmType.UCS => new UCS(),
                AlgorithmType.IDS => new IDS(),
                AlgorithmType.GBFS => new GBFS(),
                AlgorithmType.Astar => new Astar(),
                AlgorithmType.None => throw new System.InvalidOperationException("No algorithm selected"),
                _ => throw new System.ArgumentException("Unsupported algorithm type")
            };
            
            _currentSearchAlgorithm = new GeneralSearch(queuingFunction, _levelLimit);
            _currentSearchAlgorithm.Search(_currentProblem);
        }
        
        public SearchProblem GetCurrentProblem() => _currentProblem;
        public GeneralSearch GetCurrentSearchAlgorithm() => _currentSearchAlgorithm;
        public AlgorithmType GetCurrentAlgorithm() => currentAlgorithmType;
        public LevelData GetCurrentLevel() => currentLevel;
    }
}