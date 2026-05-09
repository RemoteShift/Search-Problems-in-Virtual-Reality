using DG.Tweening;
using Search.Controllers;
using UnityEngine;
using UnityEngine.UI;

public class SearchControllerUI : MonoBehaviour
{
    [SerializeField] private Dropdown timeScaleDropdown;

    // private void Update()
    // {
    //     Debug.Log($"Time Scale: {DOTween.timeScale}. Automatic Search: {SearchController.Instance.isAutomaticSearch}" +
    //               $" DOTween Tweens: {DOTween.TotalPlayingTweens()}");
    // }

    public void SetTimeScale(int index)
    {
        if (DOTween.timeScale == 0f)
        {
            return; // Don't change the timeScale if animations are currently paused
        }
        
        DOTween.timeScale = index switch
        {
            0 => 1f,
            1 => 2f,
            2 => 3f,
            3 => 5f,
            4 => 10f,
            _ => 1f
        };;
    }

    public void SetAutomaticSearch(bool value)
    {
        DOTween.Complete("Search");
        
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
            1 => 2f,
            2 => 3f,
            3 => 5f,
            4 => 10f,
            _ => 1f
        };
    }

    public void SkipAnimations()
    {
        DOTween.Complete("Search");
        SearchController.Instance.skipDelay = true;
    }
}
