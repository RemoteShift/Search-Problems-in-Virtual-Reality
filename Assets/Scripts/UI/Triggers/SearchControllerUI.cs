using DG.Tweening;
using Search.Controllers;
using UnityEngine;
using UnityEngine.UI;

public class SearchControllerUI : MonoBehaviour
{
    [SerializeField] private Dropdown timeScaleDropdown;
    
    public void SetTimeScale(int index)
    {
        DOTween.timeScale = index switch
        {
            0 => 1f,
            1 => 0.25f,
            2 => 0.5f,
            3 => 2f,
            4 => 5f,
            5 => 10f,
            _ => 1f
        };
    }

    public void SetAutomaticSearch(bool value)
    {
        SearchController.Instance.SetAutomaticSearch(value);
    }
    
    public void PauseAnimations()
    {
        DOTween.timeScale = 0;
    }
    
    public void ResumeAnimations()
    {
        DOTween.timeScale = timeScaleDropdown.value switch
        {
            0 => 1f,
            1 => 0.25f,
            2 => 0.5f,
            3 => 2f,
            4 => 5f,
            5 => 10f,
            _ => 1f
        };
    }

    public void SkipAnimations()
    {
        DOTween.CompleteAll();
    }
}
