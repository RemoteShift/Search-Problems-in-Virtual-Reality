using Search.Core;
using Search.GameModes;
using Search.Levels;
using Search.Visualization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class QueueElementUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public NodeVisual nodeVisual;
    /// <summary>
    /// If this element is in the vicinity of the queue, then this is true, else false.
    /// </summary>
    public bool isDroppingIntoQueue;
    [SerializeField] private TextMeshProUGUI nodeStateText;
    public Color32 textColor
    {
        get => nodeStateText.color;
        set => nodeStateText.color = value;
    }

    public void Initialize(NodeVisual nodeVisualInitial, Color32? color = null)
    {
        nodeVisual = nodeVisualInitial;
        nodeStateText.text = nodeVisualInitial.stateId;
        if(color != null)
            textColor = color.Value;
    }

    private void OnDestroy()
    {
        VRInputHandler.Instance.OnRightGridAndBPressChanged -= HandleNodeGrabbedQueue;
    }

    [SerializeField] private GameObject queueElementPrefab;
    private GameObject currentlyGrabbedQueueElementObject
    {
        get => PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject;
        set => PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject = value;
    }
    
    public void HandleNodeGrabbedQueue(bool value)
    {
        QueueElementUI currentlyGrabbedQueueElement;

        if (!value)
        {
            currentlyGrabbedQueueElement = currentlyGrabbedQueueElementObject?.GetComponent<QueueElementUI>();
            if (!(currentlyGrabbedQueueElement?.isDroppingIntoQueue ?? true))
            {
                Destroy(currentlyGrabbedQueueElementObject);
                currentlyGrabbedQueueElementObject = null;
            }
            
            VRInputHandler.Instance.OnRightGridAndBPressChanged -= HandleNodeGrabbedQueue;
            return;
        }

        var parentTransform = PlayerLocomotion.Instance.tempQueueHandAttachmentPoint;

        currentlyGrabbedQueueElementObject = Instantiate(queueElementPrefab, parentTransform);
        currentlyGrabbedQueueElementObject.GetComponent<Collider>().enabled = true;
        currentlyGrabbedQueueElementObject.layer = LayerMask.NameToLayer("Default");
        currentlyGrabbedQueueElement = currentlyGrabbedQueueElementObject.GetComponent<QueueElementUI>();

        currentlyGrabbedQueueElement.Initialize(nodeVisual, 
            nodeVisual.frontierMat.color); // Frontier node color

        SearchModeController.Instance.PlayerQueueState.RemoveFromPlayerFrontier(nodeVisual);
        
        EdgeManager.Instance.AddEdge("GrabbedNodeVisual", "TempQueueElementUI", 
            nodeVisual.transform, currentlyGrabbedQueueElement.transform);

        VRInputHandler.Instance.OnRightGridAndBPressChanged += currentlyGrabbedQueueElement.HandleNodeGrabbedQueue;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        NodeStatsUI.Instance.EnableAndUpdateNodeStatsUI(nodeVisual);
        var visual = LevelManager.Instance.TreeVisualizer?.GetNodeVisual(nodeVisual.SearchNode);
        if(visual)
            EdgeManager.Instance.AddEdge("QueueElementUI", "ResultingNodeVisual", 
            transform, visual.transform);
        
        if(nodeVisual.currentState == NodeState.Default
           && SearchModeController.Instance.CurrentMode == SearchPlayMode.Solve
           && !currentlyGrabbedQueueElementObject)
            VRInputHandler.Instance.OnRightGridAndBPressChanged += HandleNodeGrabbedQueue;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NodeStatsUI.Instance.DisableNodeStatsUI();
        EdgeManager.Instance.RemoveEdge("QueueElementUI", "ResultingNodeVisual");
        
        if(nodeVisual.currentState == NodeState.Default
           && SearchModeController.Instance.CurrentMode == SearchPlayMode.Solve
           && !currentlyGrabbedQueueElementObject)
            VRInputHandler.Instance.OnRightGridAndBPressChanged -= HandleNodeGrabbedQueue;
    }
}
