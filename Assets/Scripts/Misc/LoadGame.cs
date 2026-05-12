using System.Collections;
using Autohand;
using Search.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadGame : Singleton<LoadGame>
{
    void Start()
    {
        // If the EventSystem is already ready, load immediately.
        // Otherwise, wait for the event.
        if (EventSystem.current && EventSystem.current.GetComponent<AutoInputModule>())
        {
            LoadMainMenu();
        }
        else
        {
            HandCanvasPointer.OnEventSystemReady.AddListener(LoadMainMenu);
        }
    }

    void LoadMainMenu()
    {
        HandCanvasPointer.OnEventSystemReady.RemoveListener(LoadMainMenu);
        StartCoroutine(WaitAndLoadMenu());
    }

    private IEnumerator WaitAndLoadMenu()
    {
        yield return new WaitForSeconds(3f); 
        SceneManager.LoadScene("Main Menu");
    }
}
