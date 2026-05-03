using Search.Levels;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LevelSelector()
    {
        LoadingManager.Instance.LoadScene(sceneIndex: 2, onComplete:() =>
        {
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
