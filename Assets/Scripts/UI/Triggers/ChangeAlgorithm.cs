using System.Collections;
using Search.Core;
using Search.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeAlgorithm : MonoBehaviour
{
    [SerializeField] private GameObject changeAlgorithmUI;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Dropdown algorithmDropdown;
    
    private Coroutine _textCoroutine;
    
    public void ToggleUI()
    {
        if (changeAlgorithmUI)
        {
            changeAlgorithmUI.SetActive(!changeAlgorithmUI.activeSelf);
            if(changeAlgorithmUI.activeSelf)
                algorithmDropdown.value = (int)LevelManager.Instance.GetCurrentAlgorithm();
        }
    }

    public void SetAlgorithm(int algorithmIndex)
    {
        LevelManager.Instance.SetCurrentAlgorithm( algorithmIndex switch
        {
            0 => AlgorithmType.BFS,
            1 => AlgorithmType.DFS,
            2 => AlgorithmType.IDS,
            3 => AlgorithmType.UCS,
            4 => AlgorithmType.GBFS,
            5 => AlgorithmType.Astar,
            _ => LevelManager.Instance.GetCurrentAlgorithm()
        });
    }

    public void ConfirmChange()
    {
        var levelManager = LevelManager.Instance;
        if (levelManager.GetCurrentSearchAlgorithm().QueueingFunction.GetType().Name
            .Equals(levelManager.GetCurrentAlgorithm().ToString()))
        {
            _textCoroutine ??= StartCoroutine(SetTextTemporarily("Already using " + 
                                                                 levelManager.GetCurrentAlgorithm()));
        }
        else
        {
            levelManager.StartSearch();
        }
    }
    
    private IEnumerator SetTextTemporarily(string text, float duration = 2f)
    {
        if (confirmationText)
        {
            var originalText = confirmationText.text;
            confirmationText.text = text;
            yield return new WaitForSeconds(duration);
            confirmationText.text = originalText;
        }

        _textCoroutine = null;
    }
}
