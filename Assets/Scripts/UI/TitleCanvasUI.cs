using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Search.Controllers;
using Search.Core;
using Search.Core.Algorithms;
using Search.Levels;
using TMPro;
using UnityEngine;

public class TitleCanvasUI : MonoBehaviour
{
    private LevelManager _levelManager;
    private SearchController _searchController;
    
    [SerializeField] private TextMeshProUGUI algorithmText;
    
    private Coroutine _algorithmSequenceCoroutine;
    
    [Tooltip("Level data to use for the algorithm sequence.")]
    [SerializeField] private LevelData levelData;
    
    [Header("Algorithm Sequence Settings")]
    
    [Tooltip("Time in seconds to wait before loading the next algorithm in the sequence.")]
    [SerializeField] private float algorithmDisplayDuration = 5f;

    private void OnEnable()
    {
        _levelManager = LevelManager.Instance;
        _searchController = SearchController.Instance;
        _levelManager.onSearchLoaded.AddListener(UpdateAlgorithmText);

        _levelManager.currentLevel = levelData;
        _levelManager.Initialize();
        _searchController.SetAutomaticSearch(true);
        _levelManager.useGraphSearch = true;
        _algorithmSequenceCoroutine = StartCoroutine(AlgorithmSequence());
        DOTween.timeScale = 5f;
    }
    
    private void OnDisable()
    {
        _levelManager.onSearchLoaded.RemoveListener(UpdateAlgorithmText);
        StopCoroutine(_algorithmSequenceCoroutine);
        _algorithmSequenceCoroutine = null;
        DOTween.timeScale = 1f;
    }

    private IEnumerator AlgorithmSequence()
    {
        AlgorithmType[] algorithmSequence =
        {
            AlgorithmType.BFS,
            AlgorithmType.DFS,
            AlgorithmType.IDS,
            AlgorithmType.UCS,
            AlgorithmType.GBFS,
            AlgorithmType.Astar
        };
        
        while (true)
        {
            foreach (var algorithmType in algorithmSequence)
            {
                _levelManager.currentAlgorithmType = algorithmType;
                _levelManager.StartSearch();
                yield return new WaitForSeconds(algorithmDisplayDuration);
            }
        }
    }
    
    private void UpdateAlgorithmText()
    {
        var algorithmType = _levelManager.GetCurrentSearchAlgorithm().QueueingFunction;
        
        var algorithmName = algorithmType switch
        {
            BFS => "Breadth-First Search",
            DFS => "Depth-First Search",
            IDS => "Iterative Deepening Search",
            UCS => "Uniform Cost Search",
            GBFS => "Greedy Best-First Search",
            Astar => "A* Search",
            _ => algorithmType.GetType().Name
        };

        algorithmText.text = algorithmName;
    }
}
