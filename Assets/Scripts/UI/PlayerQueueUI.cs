using System.Collections.Generic;
using Search.GameModes;
using Search.Levels;
using Search.Utils;
using Search.Visualization;
using UnityEngine;

public class PlayerQueueUI : Singleton<PlayerQueueUI>
{
    [SerializeField] private GameObject queueElementPrefab;
    [SerializeField] private Transform queueContent;
    [SerializeField] private GameObject cube;

    public QueueElementUI currentlyGrabbedQueueElement;
    
    private Canvas _canvas;
    private readonly Dictionary<NodeVisual, QueueElementUI> _uiLookup = new();

    private void OnEnable()
    {
        LevelManager.Instance.onSearchLoaded.AddListener(ClearUI);
        SearchModeController.Instance.onModeChanged.AddListener(RepopulateUI);
    }

    private void OnDisable()
    {
        LevelManager.Instance?.onSearchLoaded.RemoveListener(ClearUI);
        SearchModeController.Instance?.onModeChanged.RemoveListener(RepopulateUI);
    }

    private void Start()
    {
        _canvas = GetComponentInChildren<Canvas>();

        if (SearchModeController.Instance.CurrentMode != SearchPlayMode.Solve)
        {
            _canvas.enabled = false;
            cube?.SetActive(false);
        }
        
        SearchModeController.Instance.onModeChanged.AddListener((mode) =>
        {
            _canvas.enabled = mode == SearchPlayMode.Solve;
            cube?.SetActive(mode == SearchPlayMode.Solve);
        });
    }

    /// <summary>
    /// Adds a single node to the end of the player queue.
    /// Useful for manual player insertion.
    /// </summary>
    public bool AddNode(NodeVisual nodeVisual, int index = -1)
    {
        if (!nodeVisual || _uiLookup.ContainsKey(nodeVisual))
                 return false;
        
        if (!SearchModeController.Instance.TryAddDeltaNodeToPlayerFrontier(nodeVisual, index))
            return false;

        var uiObject = Instantiate(queueElementPrefab, queueContent);
        var ui = uiObject.GetComponent<QueueElementUI>();
        ui.Initialize(nodeVisual);

        _uiLookup.Add(nodeVisual, ui);
        ui.transform.SetSiblingIndex(index >= 0 ? index : 0);
        
        return true;
    }

    /// <summary>
    /// Removes the first UI node in player queue.
    /// </summary>
    public void RemoveFirstNode()
    {
        if (queueContent.childCount == 0)
            return;

        var uiTransform = queueContent.GetChild(0);
        var element = uiTransform.GetComponent<QueueElementUI>();
        if (element && element.nodeVisual)
            _uiLookup.Remove(element.nodeVisual);

        Destroy(uiTransform.gameObject);
    }

    /// <summary>
    /// Removes a specific node from player queue UI.
    /// </summary>
    public bool RemoveNode(NodeVisual nodeVisual)
    {
        if (!nodeVisual)
            return false;

        if (!_uiLookup.TryGetValue(nodeVisual, out var ui))
            return false;

        _uiLookup.Remove(nodeVisual);
        Destroy(ui.gameObject);
        return true;
    }
    

    /// <summary>
    /// Returns current UI order as NodeVisual list.
    /// </summary>
    public List<NodeVisual> GetCurrentOrder()
    {
        var order = new List<NodeVisual>();

        for (var i = 0; i < queueContent.childCount; i++)
        {
            var child = queueContent.GetChild(i);
            var element = child.GetComponent<QueueElementUI>();
            if (element != null && element.nodeVisual)
                order.Add(element.nodeVisual);
        }

        return order;
    }

    private void RepopulateUI(SearchPlayMode mode)
    {
        if (mode != SearchPlayMode.Solve) return;
        var frontier = SearchModeController.Instance.PlayerQueueState.expectedFrontier;
        
        ClearUI();

        if (frontier == null)
            return;

        for (int i = 0; i < frontier.Count; i++)
        {
            AddNode(frontier[i], i);
        }
    }
    
    private void ClearUI()
    {
        foreach (Transform child in queueContent)
            Destroy(child.gameObject);

        _uiLookup.Clear();
    }
}