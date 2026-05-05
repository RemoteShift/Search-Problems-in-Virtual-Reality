using Search.Core.Algorithms;
using Search.Levels;
using Search.Utils;
using TMPro;
using UnityEngine;

public class StatsUI : Singleton<StatsUI>
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI searchAlgorithmText;
    [SerializeField] private GameObject levelLimitObject;
    [SerializeField] private TextMeshProUGUI levelLimitText;
    [SerializeField] private TextMeshProUGUI currLevelLimitText;
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
    
    public void UpdateStatsUI()
    {
        if (!_levelManager) return;
        
        var algorithmType = _levelManager.GetCurrentSearchAlgorithm().QueueingFunction;
        
        var algorithmName = algorithmType switch
        {
            Astar => "A*",
            _ => algorithmType.GetType().Name
        };

        _generalSearch = _levelManager.GetCurrentSearchAlgorithm();

        if (algorithmType is not IDS)
        {
            levelLimitObject.gameObject.SetActive(false);
            nodesWrongfullyExpandedText.gameObject.SetActive(false);
        }
        else
        {
            levelLimitObject.gameObject.SetActive(true);
            nodesWrongfullyExpandedText.gameObject.SetActive(true);
        }
        
        searchAlgorithmText.text = $"Search Algorithm: {algorithmName}";
        levelLimitText.text = $"{_generalSearch.LevelLimit}";
        currLevelLimitText.text = $"Curr. Level Limit: {_generalSearch.currentLevelLimit}";
        nodesGeneratedText.text = $"Nodes Generated: {_generalSearch.totalNodesGenerated}";
        nodesExpandedText.text = $"Nodes Expanded: {_generalSearch.expandedCount}";
        nodesWrongfullyExpandedText.text = $"Wrongly Expanded Nodes: {_generalSearch.wrongfullyExpandedCount}";
    }
}
