using Search.Levels;
using TMPro;
using UnityEngine;

public class SearchProblemStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startStateText;
    [SerializeField] private GameObject goalStatesContent;
    [SerializeField] private GameObject goalStateTextPrefab;
    
    private void OnEnable()
    {
        LevelManager.Instance.onLevelDataChanged.AddListener(UpdateStats);
    }

    private void OnDisable()
    {
        LevelManager.Instance.onLevelDataChanged.RemoveListener(UpdateStats);
    }

    private void UpdateStats(LevelData levelData)
    {
        startStateText.text = levelData.startState.id;
        
        foreach (Transform child in goalStatesContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var goalState in levelData.goalStates)
        {
            var goalStateText =  Instantiate(goalStateTextPrefab, goalStatesContent.transform)
                        .GetComponent<TextMeshProUGUI>();

            goalStateText.text = goalStatesContent.transform.childCount == 1 ? goalState.id : $"{goalState.id}&";
        }
        
    }
}
