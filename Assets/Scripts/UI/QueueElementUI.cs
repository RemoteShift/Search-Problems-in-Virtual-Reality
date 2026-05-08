using Search.Levels;
using Search.Visualization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class QueueElementUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public NodeVisual nodeVisual;
    private TextMeshProUGUI _nodeStateText;

    public void Initialize(NodeVisual nodeVisualInitial)
    {
        nodeVisual = nodeVisualInitial;
        _nodeStateText = GetComponentInChildren<TextMeshProUGUI>();
        _nodeStateText.text = nodeVisualInitial.stateId;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        NodeStatsUI.Instance.EnableAndUpdateNodeStatsUI(nodeVisual);
        var visual = LevelManager.Instance.TreeVisualizer?.GetNodeVisual(nodeVisual.SearchNode);
        if(visual)
            EdgeManager.Instance.AddEdge("QueueElementUI", "ResultingNodeVisual", 
            transform, visual.transform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        NodeStatsUI.Instance.DisableNodeStatsUI();
        EdgeManager.Instance.RemoveEdge("QueueElementUI", "ResultingNodeVisual");
    }
}
