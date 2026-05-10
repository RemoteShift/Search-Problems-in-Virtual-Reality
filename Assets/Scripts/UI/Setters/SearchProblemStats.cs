using Search.Levels;
using TMPro;
using UnityEngine;

public class SearchProblemStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI levelDescriptionText;
    [SerializeField] private TextMeshProUGUI startStateText;
    [SerializeField] private Transform goalStatesContent;
    [SerializeField] private GameObject goalStateTextPrefab;
    
    private void OnEnable()
    {
        LevelManager.Instance?.onLevelDataChanged.AddListener(UpdateStats);
    }

    private void OnDisable()
    {
        LevelManager.Instance?.onLevelDataChanged.RemoveListener(UpdateStats);
    }

    private void UpdateStats(LevelData levelData)
    {
        if (!levelData)
            return;

        // startState/goalStates are populated in CreateSearchProblem for current level assets.
        if (levelData.startState == null || levelData.goalStates == null)
        {
            levelData.CreateSearchProblem();
        }

        levelNameText.text = levelData.levelName;
        levelDescriptionText.text = levelData.description;
        startStateText.text = levelData.startState?.id ?? "-";
        
        foreach (Transform child in goalStatesContent)
        {
            Destroy(child.gameObject);
        }

        if (levelData.goalStates == null)
            return;

        for (var i = 0; i < levelData.goalStates.Count; i++)
        {
            var goalState = levelData.goalStates[i];
            var goalStateText = Instantiate(goalStateTextPrefab, goalStatesContent)
                .GetComponent<TextMeshProUGUI>();

            if (!goalStateText)
            {
                Debug.LogWarning("SearchProblemStats: goalStateTextPrefab is missing a TextMeshProUGUI component.");
                continue;
            }

            var isFirst = i == 0;
            goalStateText.text = isFirst ? goalState.id : $"{goalState.id} &";
        }
    }
}
