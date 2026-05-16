using Search.Levels;
using Search.Visualization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

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
