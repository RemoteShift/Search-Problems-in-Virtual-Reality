using Search.Levels;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public void Load()
    {
        LoadingManager.Instance.LoadScene(sceneName: "Level Selector", onComplete:() =>
         { LeftControllerUI.IsUIToggleable = true; 
         LevelManager.Instance.isMainMenu = false; 
         LevelManager.Instance.Initialize(); 
         LevelManager.Instance.StartSearch(); 
        });
    }
}
