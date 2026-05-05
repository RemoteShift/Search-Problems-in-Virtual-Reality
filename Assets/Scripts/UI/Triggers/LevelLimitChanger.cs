using Search.Levels;
using UnityEngine;

public class LevelLimitChanger : MonoBehaviour
{
    public void IncrementLevelLimit()
    {
        LevelManager.Instance.GetCurrentSearchAlgorithm().LevelLimit++;
        StatsUI.Instance.UpdateStatsUI();
    }
    
    public void DecrementLevelLimit()
    {
        var currentSearchAlgorithm = LevelManager.Instance.GetCurrentSearchAlgorithm();
        
        if (currentSearchAlgorithm.LevelLimit > 0)
        {
            currentSearchAlgorithm.LevelLimit--;
            StatsUI.Instance.UpdateStatsUI();
        }
    }
}
