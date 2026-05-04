using Autohand;
using Search.Levels;
using Unity.XR.CoreUtils;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private LeftControllerUI leftControllerUI;
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LevelSelector()
    {
        LoadingManager.Instance.LoadScene(sceneIndex: 2, onComplete:() =>
        {
            leftControllerUI.isUIToggleable = true;
            LevelManager.Instance.visualizeTree = true;
            LevelManager.Instance.Initialize();
            LevelManager.Instance.StartSearch();
            Destroy(gameObject);
        });
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
