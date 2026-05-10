using DG.Tweening;
using UnityEngine;
using Search.Core;
using Search.Core.Algorithms;
using Search.Utils;
using Search.Visualization;
using UnityEngine.Events;

namespace Search.Levels
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Level Settings")] [SerializeField]
        private LevelData currentLevel;
        [HideInInspector] public UnityEvent<LevelData> onLevelDataChanged = new();
        
        public bool isMainMenu = true;

        [Header("Search Algorithm Settings")]
        private AlgorithmType _currentAlgorithmType = AlgorithmType.BFS;
        [HideInInspector] public UnityEvent onAlgorithmChanged = new();
        
        [Tooltip("Whether to use graph search (track explored states and avoid duplicates in frontier) " +
                 "or tree search (allow duplicates in frontier).")]
        public bool useGraphSearch;
        
        [Tooltip("For algorithms like IDS, this sets the maximum depth limit. Ignored for other algorithms.")]
        public int levelLimit = 5;

        [Tooltip("Live Search Algorithm Instance")]
        public GeneralSearch searchAlgorithm;

        [Header("Scene References")] 
        public GameObject problemVisualizer;
        public GameObject treeVisualizer;
        public IVisualizer ProblemVisualizer;
        [HideInInspector] public TreeVisualizer TreeVisualizer;

        private EdgeManager _edgeManager;
        
        private SearchProblem _problem;
        private Coroutine _searchCoroutine;

        [HideInInspector] public UnityEvent onSearchLoaded;
        [HideInInspector] public UnityEvent onLevelLoaded;
        
        public void Initialize()
        {
            if (problemVisualizer)
                ProblemVisualizer = problemVisualizer.GetComponent<IVisualizer>();
            if (treeVisualizer)
                TreeVisualizer = treeVisualizer.GetComponent<TreeVisualizer>();
            _edgeManager = EdgeManager.Instance;

            if (currentLevel)
                LoadLevel();
            else
                Debug.LogWarning("No level assigned to LevelManager");
        }

        public void StartSearch()
        {
            ProblemVisualizer?.ClearNodeVisuals();
            TreeVisualizer?.ClearNodeVisuals();
            if (_currentAlgorithmType == AlgorithmType.IDS)
                StartSearchCoroutine(levelLimit);
            else
                StartSearchCoroutine();
        }

        public void LoadLevel()
        {
            _problem = currentLevel.CreateSearchProblem();

            ProblemVisualizer?.ClearVisuals();
            TreeVisualizer?.ClearVisuals();
            _edgeManager.ClearEdges();

            if (ProblemVisualizer is MazeVisualizer mazeVisualizer && currentLevel is MazeLevelData mazeLevel)
            {
                mazeVisualizer.Setup(mazeLevel, _problem);
            }
            
            if (!isMainMenu)
            {
                TreeVisualizer?.Setup(currentLevel, _problem);
            }
            
            onLevelLoaded.Invoke();
        }
        
        public void UnloadLevel()
        {
            ProblemVisualizer?.ClearVisuals();
            TreeVisualizer?.ClearVisuals();
            _edgeManager.ClearEdges();
        }

        private void StartSearchCoroutine(int? levelLimitValue = null)
        {
            DOTween.Complete("Search");
            
            if (_problem == null)
            {
                Debug.LogError("No search problem loaded. Cannot start search.");
                return;
            }

            if (_searchCoroutine != null)
                StopCoroutine(_searchCoroutine);

            IQueuingFunction queuingFunction = _currentAlgorithmType switch
            {
                AlgorithmType.BFS => new BFS(),
                AlgorithmType.DFS => new DFS(),
                AlgorithmType.UCS => new UCS(),
                AlgorithmType.IDS => new IDS(),
                AlgorithmType.GBFS => new GBFS(),
                AlgorithmType.Astar => new Astar(),
                _ => throw new System.ArgumentException("Unsupported algorithm type")
            };

            if (searchAlgorithm is { LevelLimit: not null })
                levelLimit = searchAlgorithm.LevelLimit.Value;
            
            searchAlgorithm = new GeneralSearch(queuingFunction, levelLimitValue ?? levelLimit, 
                currentLevel.expansionLimit);
            onSearchLoaded.Invoke();
            _searchCoroutine = StartCoroutine(searchAlgorithm.SearchCoroutine(_problem, this));
        }
        
        public SearchProblem GetCurrentProblem() => _problem;
        public LevelData GetCurrentLevelData() => currentLevel;
        public void SetCurrentLevelData(LevelData levelData)
        {
            currentLevel = levelData;
            onLevelDataChanged.Invoke(levelData);
        }

        public GeneralSearch GetCurrentSearchAlgorithm() => searchAlgorithm;
        public AlgorithmType GetCurrentAlgorithm() => _currentAlgorithmType;
        public void SetCurrentAlgorithm(AlgorithmType algorithmType)
        {
            _currentAlgorithmType = algorithmType;
            onAlgorithmChanged.Invoke();
        }
    }
}