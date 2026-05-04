using Search.Core;
using Search.Core.Algorithms;
using Search.Levels;
using TMPro;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI searchAlgorithmText;
    [SerializeField] private TextMeshProUGUI levelLimitText;
    [SerializeField] private TextMeshProUGUI nodesGeneratedText;
    [SerializeField] private TextMeshProUGUI nodesExpandedText;
    [SerializeField] private TextMeshProUGUI nodesWrongfullyExpandedText;

    private LevelManager _levelManager;
    private GeneralSearch _generalSearch;
    
    private void OnEnable()
    {
        _levelManager = LevelManager.Instance;
        _generalSearch = _levelManager.GetCurrentSearchAlgorithm();

        _generalSearch.onStepCompleted.AddListener(UpdateStatsUI);
        _levelManager.onAlgorithmChanged.AddListener(UpdateStatsUI);
        
        _levelManager.onSearchLoaded.AddListener(OnSearchLoaded);
        
        UpdateStatsUI();
    }

    private void OnDisable()
    {
        _generalSearch.onStepCompleted.RemoveListener(UpdateStatsUI);
        _levelManager.onAlgorithmChanged.RemoveListener(UpdateStatsUI);
        
        _levelManager.onSearchLoaded.RemoveListener(OnSearchLoaded);
    }
    
    private void OnSearchLoaded()
    {
        _generalSearch.onStepCompleted.RemoveListener(UpdateStatsUI);
        _generalSearch = _levelManager.GetCurrentSearchAlgorithm();
        _generalSearch.onStepCompleted.AddListener(UpdateStatsUI);
    }
    
    private void UpdateStatsUI()
    {
        if (!_levelManager) return;
        
        var algorithmType = _levelManager.GetCurrentAlgorithm();
        
        var algorithmName = algorithmType switch
        {
            AlgorithmType.Astar => "A*",
            _ => algorithmType.ToString()
        };

        _generalSearch = _levelManager.GetCurrentSearchAlgorithm();

        if (algorithmType != AlgorithmType.IDS)
        {
            levelLimitText.gameObject.SetActive(false);
            nodesWrongfullyExpandedText.gameObject.SetActive(false);
        }
        else
        {
            levelLimitText.gameObject.SetActive(true);
            nodesWrongfullyExpandedText.gameObject.SetActive(true);
        }
        
        searchAlgorithmText.text = $"Search Algorithm: {algorithmName}";
        levelLimitText.text = $"Level Limit: {_levelManager.levelLimit}";
        nodesGeneratedText.text = $"Nodes Generated: {_generalSearch.totalNodesGenerated}";
        nodesExpandedText.text = $"Nodes Expanded: {_generalSearch.expandedCount}";
        nodesWrongfullyExpandedText.text = $"Wrongly Expanded Nodes: {_generalSearch.wrongfullyExpandedCount}";
    }
}
