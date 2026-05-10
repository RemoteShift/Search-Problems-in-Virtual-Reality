using Search.Core;
using Search.Levels;
using UnityEngine;

public class SetCurrentAlgorithm : MonoBehaviour
{
    [SerializeField] private GameObject levelLimit;
    
    private void Start()
    {
        SetAlgorithm(0);
    }

    public void SetAlgorithm(int algorithmIndex)
    {
        levelLimit.SetActive(algorithmIndex == 2);

        LevelManager.Instance.SetCurrentAlgorithm( algorithmIndex switch
        {
            0 => AlgorithmType.BFS,
            1 => AlgorithmType.DFS,
            2 => AlgorithmType.IDS,
            3 => AlgorithmType.UCS,
            4 => AlgorithmType.GBFS,
            5 => AlgorithmType.Astar,
            _ => LevelManager.Instance.GetCurrentAlgorithm()
        });
    }
}
