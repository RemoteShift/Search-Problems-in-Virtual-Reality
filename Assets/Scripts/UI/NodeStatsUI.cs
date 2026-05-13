using System.Collections;
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
    [Tooltip("Duration for which the UI will stay active after being enabled.")]
    [SerializeField] private float disableDuration = 1f;
    
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
        StopAllCoroutines();
        UpdateNodeVisual(nodeVisual);
        UpdateUI(nodeVisual);
        
        transform.GetChild(0).gameObject.SetActive(true);
    }
    
    public void DisableNodeStatsUI()
    {
        StartCoroutine(DisableUI(disableDuration));
    }

    private IEnumerator DisableUI(float duration)
    {
        yield return new WaitForSeconds(duration);
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
        
        pathCostText.text = nodeVisual.SearchNode.pathCost + "";
        
        if (algorithmType is Astar or UCS)
        {
            pathCostText.transform.parent.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }
        else
        {
            pathCostText.transform.parent.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
        }

        heuristicCostText.text = nodeVisual.SearchNode.heuristicCost + "";
        
        if (algorithmType is Astar or GBFS)
        {
            heuristicCostText.transform.parent.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }
        else
        {
            heuristicCostText.transform.parent.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
        }

        fCostText.text = nodeVisual.SearchNode.F + "";
        
        if (algorithmType is Astar)
        {
            fCostText.transform.parent.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
        }
        else
        {
            fCostText.transform.parent.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Strikethrough;
        }
    }
}
