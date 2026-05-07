using DG.Tweening;
using Search.Controllers;
using Search.Levels;
using UnityEngine;

public class RestartSearch : MonoBehaviour
{
    public void Restart()
    {
        DOTween.CompleteAll();
        LevelManager.Instance.StartSearch();
    }
}
