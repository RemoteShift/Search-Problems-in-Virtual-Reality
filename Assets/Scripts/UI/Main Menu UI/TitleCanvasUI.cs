using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using Search.Controllers;
using Search.Core;
using Search.Core.Algorithms;
using Search.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleCanvasUI : AnimatableUI
{
    private LevelManager _levelManager;
    private SearchController _searchController;
    
    [SerializeField] private TextMeshProUGUI algorithmText;

    #region Initial Rect Mask Padding Settings

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private RectMask2D rectMask;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float left;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float right;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float top;

    [Foldout("Initial Rect Mask Padding Settings")] [SerializeField]
    private float bottom;

    #endregion
    
    private Coroutine _algorithmSequenceCoroutine;
    
    [Tooltip("Level data to use for the algorithm sequence.")]
    [SerializeField] private LevelData levelData;
    
    [Header("Algorithm Sequence Settings")]
    
    [Tooltip("Time in seconds to wait before loading the next algorithm in the sequence.")]
    [SerializeField] private float algorithmDisplayDuration = 5f;

    public override Sequence PlayOpen()
    {
        _levelManager = LevelManager.Instance;
        _searchController = SearchController.Instance;
        _levelManager.onSearchLoaded.AddListener(UpdateAlgorithmText);

        _levelManager.currentLevel = levelData;
        _levelManager.Initialize();
        _searchController.SetAutomaticSearch(true);
        _levelManager.useGraphSearch = true;

        #region Animation

        var seq = DOTween.Sequence().SetId("UI");
        
        #region Animate Rect Mask Padding Open
        
        rectMask.padding = new Vector4(left, bottom, right, top);
        
        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    0,
                    rectMask.padding.y,
                    rectMask.padding.z,
                    rectMask.padding.w
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );

        seq.AppendInterval(0.1f);
        
        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    0,
                    0,
                    rectMask.padding.z,
                    0
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );
        
        #endregion
        
        seq.OnComplete(() =>
        {
            _algorithmSequenceCoroutine = StartCoroutine(AlgorithmSequence());
            DOTween.timeScale = 5f;
        });

        return seq;

        #endregion
    }
    
    public override Sequence PlayClose()
    {
        #region Animation

        var seq = DOTween.Sequence().SetId("UI");

        #region Animate Rect Mask Padding Close

        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    rectMask.padding.x,
                    bottom,
                    rectMask.padding.z,
                    top
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );
        
        seq.Append(
            DOTween.To(
                () => rectMask.padding,
                x => rectMask.padding = x,
                new Vector4(
                    left,
                    bottom,
                    rectMask.padding.z,
                    top
                ),
                0.2f
            ).SetEase(Ease.OutCubic).SetId("UI")
        );

        #endregion

        seq.OnComplete(() =>
        {
            _levelManager.onSearchLoaded.RemoveListener(UpdateAlgorithmText);
            StopCoroutine(_algorithmSequenceCoroutine);
            _algorithmSequenceCoroutine = null;
            DOTween.timeScale = 1f;
            gameObject.SetActive(false);
        });

        return seq;

        #endregion
    }

    private IEnumerator AlgorithmSequence()
    {
        AlgorithmType[] algorithmSequence =
        {
            AlgorithmType.BFS,
            AlgorithmType.DFS,
            AlgorithmType.IDS,
            AlgorithmType.UCS,
            AlgorithmType.GBFS,
            AlgorithmType.Astar
        };
        
        while (true)
        {
            foreach (var algorithmType in algorithmSequence)
            {
                _levelManager.currentAlgorithmType = algorithmType;
                _levelManager.StartSearch();
                yield return new WaitForSeconds(algorithmDisplayDuration);
            }
        }
    }
    
    private void UpdateAlgorithmText()
    {
        var algorithmType = _levelManager.GetCurrentSearchAlgorithm().QueueingFunction;
        
        var algorithmName = algorithmType switch
        {
            BFS => "Breadth-First Search",
            DFS => "Depth-First Search",
            IDS => "Iterative Deepening Search",
            UCS => "Uniform Cost Search",
            GBFS => "Greedy Best-First Search",
            Astar => "A* Search",
            _ => algorithmType.GetType().Name
        };

        algorithmText.text = algorithmName;
    }
}
