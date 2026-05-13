using System.Collections.Generic;
using Search.Controllers;
using Search.Core.Algorithms;
using Search.Levels;
using Search.Visualization;
using Search.GameModes;
using UnityEngine;

public class QueueUI : MonoBehaviour
{
    [SerializeField] private GameObject queueElementPrefab;
    [SerializeField] private Transform queueContent;
    
    private Dictionary<NodeVisual, QueueElementUI> _uiLookup = new ();

    private void OnEnable()
    {
        SearchController.Instance.queueUI = this;
        LevelManager.Instance.onSearchLoaded.AddListener(ClearUI);
    }

    private void OnDisable()
    {
        SearchController.Instance.queueUI = null;
        LevelManager.Instance?.onSearchLoaded.RemoveListener(ClearUI);
    }

    /// <summary>
    /// Adds the nodes to the frontier queue UI. Call this when nodes are added to the frontier.
    /// </summary>
    /// <param name="nodes">List of the nodes to be added</param>
    public void AddNodes(List<NodeVisual> nodes)
    {
        var queueingFunction = LevelManager.Instance.GetCurrentSearchAlgorithm().QueueingFunction;

        foreach (var nodeVisual in nodes)
        {
            GameObject uiObject;
            uiObject = Instantiate(queueElementPrefab, queueContent);
            var ui = uiObject.GetComponent<QueueElementUI>();
            ui.Initialize(nodeVisual);
            _uiLookup.Add(nodeVisual, ui);
            
            if (queueingFunction is DFS or IDS)
            {
                ui.gameObject.transform.SetAsFirstSibling();
            }
            else
            {
                ui.gameObject.transform.SetAsLastSibling();
            }
        }
    }
    
    /// <summary>
    /// Removes the first node from the frontier queue UI. Call this when a node is removed from the frontier.
    /// </summary>
    public void RemoveFirstNode()
    {
        if(queueContent.childCount == 0) return;
        var uiTransform = queueContent.transform.GetChild(0);
        var nodeVisual = uiTransform.GetComponent<QueueElementUI>().nodeVisual;
        _uiLookup.Remove(nodeVisual);
        Destroy(uiTransform.gameObject);
    }
    
    /// <summary>
    /// Syncs ONLY the visual order of the frontier UI.
    /// Assumes UI objects already exist and match the frontier list.
    /// </summary>
    public void SyncFrontierOrder(List<NodeVisual> frontier)
    {
        for (var i = 0; i < frontier.Count; i++)
        {
            var node = frontier[i];

            if (_uiLookup.TryGetValue(node, out var ui))
            {
                var currentIndex = ui.transform.GetSiblingIndex();

                if (currentIndex != i)
                {
                    ui.transform.SetSiblingIndex(i);
                }
            }
        }
    }
    
    private void ClearUI()
    {
        foreach (Transform child in queueContent)
        {
            Destroy(child.gameObject);
        }
        _uiLookup.Clear();
    }
    
    public bool TryAddDeltaNodeToPlayerFrontier(NodeVisual nodeVisual)
    {
        return SearchModeController.Instance.TryAddDeltaNodeToPlayerFrontier(nodeVisual);
    }
}
