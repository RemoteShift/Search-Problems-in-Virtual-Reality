using Search.Levels;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{
    public void Load()
    {
        LoadingManager.Instance.LoadScene(sceneIndex: 2, onComplete:() =>
         { LeftControllerUI.IsUIToggleable = true; 
         LevelManager.Instance.isMainMenu = false; 
         LevelManager.Instance.Initialize(); 
         LevelManager.Instance.StartSearch(); 
        });
    }
}
