using System;
using Search.Core;
using Search.Levels;
using Search.Utils;
using TMPro;
using UnityEngine;

public class SearchResultUI : Singleton<SearchResultUI>
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI goalStateText;
    [SerializeField] private TextMeshProUGUI timeElapsedText;
    [SerializeField] private TextMeshProUGUI nodesExpandedText;
    [SerializeField] private TextMeshProUGUI nodesGeneratedText;
    [SerializeField] private TextMeshProUGUI reasonOfFailureText;
    
    private void OnEnable()
    {
        LevelManager.Instance.onSearchLoaded.AddListener(DisableUI);
    }

    private void OnDisable()
    {
        LevelManager.Instance?.onSearchLoaded.RemoveListener(DisableUI);
    }

    public void DisplayResult(SearchResult result)
    {
        UpdateUI(result);
        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void UpdateUI(SearchResult result)
    {
        resultText.text = result.success ? "<color=green>Success!</color>" : "<color=red>Failure</color>";

        if (result.success)
        {
            goalStateText.text = result.solutionNode.state.id;
            goalStateText.transform.parent.gameObject.SetActive(true);
            reasonOfFailureText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            reasonOfFailureText.text = result.failureReason;
            reasonOfFailureText.transform.parent.gameObject.SetActive(true);
            goalStateText.transform.parent.gameObject.SetActive(false);
        }
        
        var time = TimeSpan.FromSeconds(result.totalTimeS);
        
        timeElapsedText.text = time.TotalMinutes < 1 ? $"{time.TotalSeconds:F2}s" : 
            $"{time.Minutes}m {time.Seconds:F2}s";
        
        nodesExpandedText.text = result.nodesExpanded.ToString();
        nodesGeneratedText.text = result.nodesGenerated.ToString();
    }
    
    private void DisableUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}
