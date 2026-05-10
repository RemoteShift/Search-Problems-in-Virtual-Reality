using Search.Levels;
using UnityEngine;

public class SetGraphSearch : MonoBehaviour
{
    public void SetUseGraphSearch(bool value)
    {
        LevelManager.Instance.useGraphSearch = value;
    }
}
