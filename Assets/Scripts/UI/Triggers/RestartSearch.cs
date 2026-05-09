using Search.Levels;
using UnityEngine;

public class RestartSearch : MonoBehaviour
{
    public void Restart()
    {
        LevelManager.Instance.StartSearch();
    }
}
