using System.Collections.Generic;
using System.Linq;
using Search.GameModes;
using Search.Levels;
using Search.Utils;
using Search.Visualization;
using UnityEngine;
using UnityEngine.UI;

public class PlayerQueueUI : Singleton<PlayerQueueUI>
{
    [SerializeField] private GameObject queueElementPrefab;
    [SerializeField] private GameObject dropZonePrefab;
    [SerializeField] private GameObject hoveringElementPrefab;
    [SerializeField] private Transform queueContent;
    [SerializeField] private Transform dropZoneContent;
    [SerializeField] private GameObject cube;
    [SerializeField] private QueueDropZone lastDropZone;

    [HideInInspector] public GameObject currentlyGrabbedQueueElementObject;
    private readonly Dictionary<QueueDropZone, int> _hoverCounts = new();
    private GameObject _hoveringElementInstance;
    private QueueDropZone _zoneToDropAt;
    
    private Canvas _canvas;
    private readonly Dictionary<NodeVisual, QueueElementUI> _uiLookup = new();
    private readonly Dictionary<QueueElementUI, QueueDropZone> _dropZoneLookup = new();
    private readonly List<QueueElementUI> _currentlyMisplacedElements = new();

    private void OnEnable()
    {
        LevelManager.Instance.onSearchLoaded.AddListener(ClearUI);
        SearchModeController.Instance.onModeChanged.AddListener(RepopulateUI);
        SearchModeController.Instance.onValidationSuccess.AddListener(SetLatestDeltaElementsTextColorWhite);
        VRInputHandler.Instance.OnRightGridAndBPressChanged += HandleFrontierAddNode;
    }

    private void OnDisable()
    {
        LevelManager.Instance?.onSearchLoaded.RemoveListener(ClearUI);
        SearchModeController.Instance?.onModeChanged.RemoveListener(RepopulateUI);
        SearchModeController.Instance?.onValidationSuccess.RemoveListener(SetLatestDeltaElementsTextColorWhite);
        var handler = VRInputHandler.Instance;
        if (handler)
        {
            handler.OnRightGridAndBPressChanged -= HandleFrontierAddNode;
        }
    }

    private void Start()
    {
        _canvas = GetComponentInChildren<Canvas>();

        if(lastDropZone)
            lastDropZone.gameObject.layer = LayerMask.NameToLayer("Default");
        
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

    public void ShowMisplacedElements(List<NodeVisual> nodes)
    {
        foreach (var kvp in _uiLookup.Where(kv => nodes.Contains(kv.Key)))
        {
            kvp.Value.textColor = Color.red;
            _currentlyMisplacedElements.Add(kvp.Value);
        }
    }

    private void ResetMisplacedElements()
    {
        foreach (var misplacedElement in _currentlyMisplacedElements)
        {
            misplacedElement.textColor = Color.white;
        }
        
        _currentlyMisplacedElements.Clear();
    }

    private void SetLatestDeltaElementsTextColorWhite()
    {
        var nodeVisuals = SearchModeController.Instance.latestDelta;
        foreach (var nodeVisual in nodeVisuals)
        {
            _uiLookup.TryGetValue(nodeVisual, out var element);
            if(element)
                element.textColor = Color.white;
        }
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
        ui.transform.SetSiblingIndex(index >= 0 ? index + 1 : 1);

        var dropZoneObject = Instantiate(dropZonePrefab, dropZoneContent);
        dropZoneObject.layer = LayerMask.NameToLayer("Default");
        var dropZone = dropZoneObject.GetComponent<QueueDropZone>();
        
        _dropZoneLookup.Add(ui, dropZone);
        dropZone.transform.SetSiblingIndex(index >= 0 ? index : 0);
        
        dropZoneObject.name = dropZone.zoneIndex.ToString();
        
        ResetMisplacedElements();
        
        return true;
    }

    private bool AddNode(GameObject elementObject, int index = -1)
    {
        if (!elementObject)
            return false;
        
        var element = elementObject.GetComponent<QueueElementUI>();
        
        if(!element || _uiLookup.ContainsKey(element.nodeVisual))
            return false;
        
        if (!SearchModeController.Instance.TryAddDeltaNodeToPlayerFrontier(element.nodeVisual, index))
            return false;
        
        var uiObject = Instantiate(queueElementPrefab, queueContent);
        var ui = uiObject.GetComponent<QueueElementUI>();
        ui.GetComponent<Collider>().enabled = false;
        ui.Initialize(element.nodeVisual, element.textColor);
        
        _uiLookup.Add(ui.nodeVisual, ui);
        ui.transform.SetSiblingIndex(index >= 0 ? index + 1 : 1);
        
        var dropZoneObject = Instantiate(dropZonePrefab, dropZoneContent);
        dropZoneObject.layer = LayerMask.NameToLayer("Default");
        var dropZone = dropZoneObject.GetComponent<QueueDropZone>();
        
        _dropZoneLookup.Add(ui, dropZone);
        dropZone.transform.SetSiblingIndex(index >= 0 ? index : 0);
        dropZoneObject.name = dropZone.zoneIndex.ToString();

        ResetMisplacedElements();
        
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
        
        ResetMisplacedElements();
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
        
        ResetMisplacedElements();
        
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

    public void RegisterHover(QueueDropZone zone)
    {
        if (zone == null) return;

        var count = _hoverCounts.GetValueOrDefault(zone, 0);

        count++;
        _hoverCounts[zone] = count;

        if (count == 1)
            UpdateHoverVisual();
    }

    public void UnregisterHover(QueueDropZone zone)
    {
        if (zone == null) return;

        if (!_hoverCounts.TryGetValue(zone, out var count))
            return;

        count--;
        if (count <= 0)
        {
            _hoverCounts.Remove(zone);
            UpdateHoverVisual(); // zone fully cleared
        }
        else
        {
            _hoverCounts[zone] = count;
        }
    }

    private void UpdateHoverVisual()
    {
        if (_hoverCounts.Count == 0)
        {
            DisableHoveringVisual();
            return;
        }

        // pick best zone; e.g. smallest zoneIndex or change to distance-based
        _zoneToDropAt = _hoverCounts.Keys.OrderBy(z => z.zoneIndex).First();
        ShowHoveringVisual(_zoneToDropAt.zoneIndex);
    }
    
    private void ShowHoveringVisual(int index)
    {
        if (_hoveringElementInstance == null)
        {
            _hoveringElementInstance = Instantiate(hoveringElementPrefab, queueContent);
        }

        _hoveringElementInstance.transform.SetSiblingIndex(index + 1);
        
        if (queueContent is RectTransform rect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }
    
    private void DisableHoveringVisual()
    {
        Destroy(_hoveringElementInstance);
        _hoveringElementInstance = null;
    }

    private void HandleFrontierAddNode(bool value)
    {
        GetComponent<Rigidbody>().isKinematic = value;
        
        if (value || !currentlyGrabbedQueueElementObject || !_hoveringElementInstance)
            return; // Element was grabbed, not dropped. Or not being hovered
        
        DisableHoveringVisual();
        _hoverCounts.Clear();
        AddNode(currentlyGrabbedQueueElementObject, _zoneToDropAt.zoneIndex);
        
        Destroy(currentlyGrabbedQueueElementObject);
        currentlyGrabbedQueueElementObject = null;
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
            var dropZone = dropZoneObject.GetComponent<QueueDropZone>();
        
            _dropZoneLookup.Add(ui, dropZone);
            dropZone.transform.SetSiblingIndex(i);
            dropZoneObject.name = i.ToString();
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
                    if(zone != lastDropZone.transform)
                        Destroy(zone.gameObject);
                }
                
                continue;
            }
            
            Destroy(child.gameObject);
        }
        
        _uiLookup.Clear();
        _dropZoneLookup.Clear();
        _currentlyMisplacedElements.Clear();
        SearchModeController.Instance.PlayerQueueState.ClearPlayerFrontier();
    }
}