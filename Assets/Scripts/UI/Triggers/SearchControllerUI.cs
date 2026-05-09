using DG.Tweening;
using Search.Controllers;
using UnityEngine;
using UnityEngine.UI;

public class SearchControllerUI : MonoBehaviour
{
    [SerializeField] private Dropdown timeScaleDropdown;

    private float searchTimeScale => SearchController.Instance.searchTimeScale;

    // private void Update()
    // {
    //     Debug.Log($"Time Scale: {searchTimeScale}. Automatic Search: {SearchController.Instance.isAutomaticSearch}" +
    //               $" DOTween Tweens: {DOTween.TotalPlayingTweens()}");
    // }

    public void SetTimeScale(int index)
    {
        if (searchTimeScale == 0f)
        {
            return; // Don't change the timeScale if animations are currently paused
        }

        SearchController.Instance.SetSearchTimeScale(index switch
        {
            0 => 1f,
            1 => 2f,
            2 => 3f,
            3 => 5f,
            4 => 10f,
            _ => 1f
        });
    }

    public void SetAutomaticSearch(bool value)
    {
        DOTween.Complete("Search");
        
        SearchController.Instance.SetAutomaticSearch(value);
    }
    
    public void PauseAnimations()
    {
        SearchController.Instance.SetSearchTimeScale(0f);
    }
    
    public void ResumeAnimations()
    {
        SearchController.Instance.SetSearchTimeScale(timeScaleDropdown.value switch
        {
            0 => 1f,
            1 => 2f,
            2 => 3f,
            3 => 5f,
            4 => 10f,
            _ => 1f
        });
    }

    public void SkipAnimations()
    {
        DOTween.Complete("Search");
        SearchController.Instance.skipDelay = true;
    }
}
