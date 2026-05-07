using DG.Tweening;
using Search.Controllers;
using UnityEngine;
using UnityEngine.UI;

public class SearchControllerUI : MonoBehaviour
{
    [SerializeField] private Dropdown timeScaleDropdown;

    private float _timeScaleChosen = 1f;
    
    public void SetTimeScale(int index)
    {
        _timeScaleChosen = index switch
        {
            0 => 1f,
            1 => 2f,
            2 => 3f,
            3 => 5f,
            4 => 10f,
            _ => 1f
        };

        if (DOTween.timeScale == 0f)
        {
            return; // Don't change the timeScale if animations are currently paused
        }
        
        DOTween.timeScale = _timeScaleChosen;
    }

    public void SetAutomaticSearch(bool value)
    {
        DOTween.KillAll();
        
        SearchController.Instance.SetAutomaticSearch(value);
    }
    
    public void PauseAnimations()
    {
        DOTween.timeScale = 0;
    }
    
    public void ResumeAnimations()
    {
        DOTween.timeScale = _timeScaleChosen;
    }

    public void SkipAnimations()
    {
        DOTween.CompleteAll();
    }
}
