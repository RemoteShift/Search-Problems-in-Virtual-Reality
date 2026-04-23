using UnityEngine;
using Search.Core;
using Search.Core.Algorithms;
using Search.Utils;
using Search.Visualization;
using UnityEngine.Serialization;

namespace Search.Levels
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Level Asset")] [SerializeField]
        private LevelData currentLevel;

        [Header("Search Algorithm Settings")]
        [SerializeField] private AlgorithmType currentAlgorithmType = AlgorithmType.None;
        
        [Tooltip("Whether to use graph search (track explored states and avoid duplicates in frontier) " +
                 "or tree search (allow duplicates in frontier).")]
        public bool useGraphSearch = false;

        [Tooltip("Whether to step through the search algorithm one expansion at a time (e.g. via UI button) " +
                 "or let it run continuously until completion.")]
        public bool isStepped = false;
        
        [Tooltip("For algorithms like IDS, this sets the maximum depth limit. Ignored for other algorithms.")]
        [SerializeField] private int levelLimit = 5;

        [Tooltip("Live Search Algorithm Instance")]
        public GeneralSearch searchAlgorithm;
        
        [Header("Scene References")]
        public MazeVisualizer mazeVisualizer;

        // For graph levels you could add GraphVisualizer later
        //[SerializeField] private VRSearchController searchController;

        private SearchProblem _problem;
        private Coroutine _searchCoroutine;

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
            _problem = level.CreateSearchProblem();
            
            if (mazeVisualizer && level is MazeLevelData mazeLevel)
            {
                mazeVisualizer.isTreeSearch = !useGraphSearch;
                mazeVisualizer.Setup(mazeLevel, _problem);
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
            if (_problem == null)
            {
                Debug.LogError("No search problem loaded. Cannot start search.");
                return;
            }

            if (_searchCoroutine != null)
                StopCoroutine(_searchCoroutine);

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
            
            searchAlgorithm = new GeneralSearch(queuingFunction, _levelLimit, 
                searchAlgorithm.expansionLimit);
            _searchCoroutine = StartCoroutine(searchAlgorithm.SearchCoroutine(_problem, this));
        }
        
        public SearchProblem GetCurrentProblem() => _problem;
        public GeneralSearch GetCurrentSearchAlgorithm() => searchAlgorithm;
        public AlgorithmType GetCurrentAlgorithm() => currentAlgorithmType;
        public LevelData GetCurrentLevel() => currentLevel;
    }
}