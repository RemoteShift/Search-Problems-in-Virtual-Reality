using System.Collections.Generic;
using Search.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetLevelBoxes : MonoBehaviour
{
    [SerializeField] private List<LevelData> levelDataList;
    [SerializeField] private Transform levelBoxContainer;
    [SerializeField] private GameObject levelBoxPrefab;
    
    private Button _firstButton;
    
    private void Start()
    {
        UpdateLevelBoxes();
        _firstButton?.onClick.Invoke();
    }

    private void UpdateLevelBoxes()
    {
        foreach (Transform child in levelBoxContainer)
        {
            Destroy(child.gameObject);
        }

        int idx = 0;
        foreach (var levelData in levelDataList)
        {
            var levelBox =  Instantiate(levelBoxPrefab, levelBoxContainer);
            var levelBoxButton = levelBox.GetComponent<Button>();
            if (idx == 0) _firstButton = levelBoxButton;
            var levelBoxButtonText = levelBox.GetComponentInChildren<TextMeshProUGUI>();

            levelBoxButtonText.text = $"{idx + 1}";

            levelBoxButton.onClick.AddListener(() =>
            {
                var levelManager = LevelManager.Instance;
                
                levelManager.SetCurrentLevelData(levelData);
                levelManager.LoadLevel();
            });
            
            idx++;
        }
    }
}
