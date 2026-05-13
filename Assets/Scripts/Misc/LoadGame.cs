using Search.Utils;
using UnityEngine.SceneManagement;

public class LoadGame : Singleton<LoadGame>
{
    void Start()
    {
        SceneManager.LoadScene("Main Menu");
    }
    
}
