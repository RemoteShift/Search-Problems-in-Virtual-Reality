using Search.Core;
using Search.Core.Algorithms;
using Search.Levels;
using Search.Utils;
using Search.Visualization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeStatsUI : Singleton<NodeStatsUI>
{
    [SerializeField] private GameObject nodeVisualObject;

    [Header("UI Elements")] 
    [SerializeField] private TextMeshProUGUI nodeStateText;
    [SerializeField] private Image nodeStateImage;
    [SerializeField] private TextMeshProUGUI stateIDText;
    [SerializeField] private TextMeshProUGUI actionFromParentText;
    [SerializeField] private TextMeshProUGUI depthText;
    [SerializeField] private TextMeshProUGUI pathCostText;
    [SerializeField] private TextMeshProUGUI heuristicCostText;
    [SerializeField] private TextMeshProUGUI fCostText;
    
    public void EnableAndUpdateNodeStatsUI(NodeVisual nodeVisual)
    {
        UpdateNodeVisual(nodeVisual);
        UpdateUI(nodeVisual);
        
        transform.GetChild(0).gameObject.SetActive(true);
    }
    
    public void DisableNodeStatsUI()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }

    private void UpdateNodeVisual(NodeVisual newVisual)
    {
        nodeVisualObject.GetComponent<MeshRenderer>().material.color = newVisual.currentState switch
        {
            NodeState.Expanding => Color.red,
            _ => newVisual.currentOriginalColor 
        };
        
        var textMesh = nodeVisualObject.GetComponentInChildren<TextMeshProUGUI>();
        textMesh.text = newVisual.stateId;
    }

    private void UpdateUI(NodeVisual nodeVisual)
    {
        var generalSearch = LevelManager.Instance.GetCurrentSearchAlgorithm();
        
        var algorithmType = generalSearch.QueueingFunction;
        
        nodeStateText.text = $"Node State: {nodeVisual.currentState.ToString()}";
        nodeStateImage.color = nodeVisual.currentState switch
        {
            NodeState.Expanding => Color.red,
            _ => nodeVisual.currentOriginalColor
        };
        stateIDText.text = nodeVisual.stateId;
        actionFromParentText.text = nodeVisual.SearchNode.actionFromParent;
        depthText.text = nodeVisual.SearchNode.depth.ToString();

        if (algorithmType is Astar or GBFS or UCS)
        {
            pathCostText.transform.parent.gameObject.SetActive(true);
            pathCostText.text = nodeVisual.SearchNode.pathCost + "";
        }
        else
        {
            pathCostText.transform.parent.gameObject.SetActive(false);
        }

        if (algorithmType is Astar or GBFS)
        {
            heuristicCostText.transform.parent.gameObject.SetActive(true);
            heuristicCostText.text = nodeVisual.SearchNode.heuristicCost + "";
            
            fCostText.transform.parent.gameObject.SetActive(true);
            fCostText.text = nodeVisual.SearchNode.F + "";
        }
        else
        {
            heuristicCostText.transform.parent.gameObject.SetActive(false);
            fCostText.transform.parent.gameObject.SetActive(false);
        }
    }
}
