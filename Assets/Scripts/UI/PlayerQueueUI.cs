using System.Collections.Generic;
using System.Linq;
using Search.GameModes;
using Search.Levels;
using Search.Utils;
using Search.Visualization;
using UnityEngine;

public class PlayerQueueUI : Singleton<PlayerQueueUI>
{
    [SerializeField] private GameObject queueElementPrefab;
    [SerializeField] private GameObject dropZonePrefab;
    [SerializeField] private Transform queueContent;
    [SerializeField] private Transform dropZoneContent;
    [SerializeField] private GameObject cube;

    [HideInInspector] public GameObject currentlyGrabbedQueueElementObject;
    private int _hoveredZoneIndex;
    
    private Canvas _canvas;
    private readonly Dictionary<NodeVisual, QueueElementUI> _uiLookup = new();
    private readonly Dictionary<QueueElementUI, QueueDropZone> _dropZoneLookup = new();

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
        ui.GetComponent<Collider>().enabled = false;
        ui.Initialize(nodeVisual);

        _uiLookup.Add(nodeVisual, ui);
        ui.transform.SetSiblingIndex(index >= 0 ? index : 0);

        var dropZoneObject = Instantiate(dropZonePrefab, dropZoneContent);
        var dropZone = dropZoneObject.GetComponent<QueueDropZone>();
        dropZone.zoneIndex = index >= 0 ? index : 0;
        
        _dropZoneLookup.Add(ui, dropZone);
        dropZone.transform.SetSiblingIndex(index >= 0 ? index : 0);
        
        return true;
    }

    /// <summary>
    /// Removes the first UI node in player queue.
    /// </summary>
    public void RemoveFirstNode()
    {
        if (queueContent.childCount == 1)
            return;

        var uiTransform = queueContent.GetChild(1);
        var element = uiTransform.GetComponent<QueueElementUI>();
        if (element && element.nodeVisual)
            _uiLookup.Remove(element.nodeVisual);

        Destroy(_dropZoneLookup[element].gameObject);
        _dropZoneLookup.Remove(element);
        
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

        Destroy(_dropZoneLookup[ui].gameObject);
        _dropZoneLookup.Remove(ui);
        
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

        for (var i = 1; i < queueContent.childCount; i++)
        {
            var child = queueContent.GetChild(i);
            var element = child.GetComponent<QueueElementUI>();
            if (element != null && element.nodeVisual)
                order.Add(element.nodeVisual);
        }

        return order;
    }

    public void SetHoveringIndex(int index)
    {
        _hoveredZoneIndex = index;

        if (index == -1)
        {
            //DisableHoveringVisual();
            return;
        }
        
        
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
            var nodeVisual = frontier[i];
            
            if(!SearchModeController.Instance.PlayerQueueState.AddToPlayerFrontier(nodeVisual, i))
                continue;
            
            var uiObject = Instantiate(queueElementPrefab, queueContent);
            var ui = uiObject.GetComponent<QueueElementUI>();
            ui.GetComponent<Collider>().enabled = false;
            ui.Initialize(nodeVisual);

            _uiLookup.Add(nodeVisual, ui);
            ui.transform.SetSiblingIndex(i + 1);

            var dropZoneObject = Instantiate(dropZonePrefab, dropZoneContent);
            dropZoneObject.layer = LayerMask.NameToLayer("Default");
            dropZoneObject.name = i.ToString();
            var dropZone = dropZoneObject.GetComponent<QueueDropZone>();
            dropZone.zoneIndex = i;
        
            _dropZoneLookup.Add(ui, dropZone);
            dropZone.transform.SetSiblingIndex(i);
        }
    }
    
    private void ClearUI()
    {
        foreach (Transform child in queueContent)
        {
            if (child.GetSiblingIndex() == 0) // Clear all the Drop Zones
            {
                foreach (Transform zone in child.GetChild(0))
                {
                    Destroy(zone.gameObject);
                }
                continue;
            }
            
            Destroy(child.gameObject);
        }
        
        _uiLookup.Clear();
        _dropZoneLookup.Clear();
        SearchModeController.Instance.PlayerQueueState.ClearPlayerFrontier();
    }
}