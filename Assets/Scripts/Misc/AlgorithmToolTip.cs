using System;
using Search.Core;
using Search.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlgorithmToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject BFSTooltipObject;
    [SerializeField] private GameObject DFSTooltipObject;
    [SerializeField] private GameObject IDSTooltipObject;
    [SerializeField] private GameObject UCSTooltipObject;
    [SerializeField] private GameObject GBFSTooltipObject;
    [SerializeField] private GameObject AstarTooltipObject;

    private GameObject _toolTipObject;
    [SerializeField] private Dropdown dropdown;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    
    private void Start()
    {
        if(!dropdown)
            dropdown = GetComponent<Dropdown>();
        if(!textMeshPro)
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        LevelManager.Instance.onAlgorithmChanged.AddListener(ChangeToolTip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        VRInputHandler.Instance.OnRightGripAndAValueChanged += ShowToolTip;
        VRInputHandler.Instance.OnRightGripValueChanged += ShowQuestionMark;
        ShowQuestionMark(VRInputHandler.Instance.rightGripValue.action.ReadValue<float>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        VRInputHandler.Instance.OnRightGripAndAValueChanged -= ShowToolTip;
        VRInputHandler.Instance.OnRightGripValueChanged -= ShowQuestionMark;
        PlayerLocomotion.Instance.questionMarkCanvasObject.SetActive(false);
    }

    private void ShowQuestionMark(float value)
    {
        PlayerLocomotion.Instance.questionMarkCanvasObject.SetActive(value > 0.1f);
    }
    
    private void ShowToolTip()
    {
        _toolTipObject?.SetActive(false);
        
        if (dropdown)
        {
            var index = dropdown.value;

            _toolTipObject = index switch
            {
                0 => BFSTooltipObject,
                1 => DFSTooltipObject,
                2 => IDSTooltipObject,
                3 => UCSTooltipObject,
                4 => GBFSTooltipObject,
                5 => AstarTooltipObject,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else if(textMeshPro)
        {
            _toolTipObject = textMeshPro.text switch
            {
                "BFS" => BFSTooltipObject,
                "DFS" => DFSTooltipObject,
                "IDS" => IDSTooltipObject,
                "UCS" => UCSTooltipObject,
                "GBFS" => GBFSTooltipObject,
                "A*" => AstarTooltipObject,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            Debug.LogError("AlgorithmToolTip: No Dropdown or TextMeshProUGUI component found on the GameObject. " +
                           "Please add one of these components for the tooltip to work.");
            return;
        }
        
        _toolTipObject.SetActive(true);
    }

    private void ChangeToolTip()
    {
        if (!_toolTipObject?.activeSelf ?? false)
            return;
        
        _toolTipObject?.SetActive(false);
        
        var algorithmType = LevelManager.Instance.GetCurrentAlgorithm();
        _toolTipObject = algorithmType switch
        {
            AlgorithmType.BFS => BFSTooltipObject,
            AlgorithmType.DFS => DFSTooltipObject,
            AlgorithmType.IDS => IDSTooltipObject,
            AlgorithmType.UCS => UCSTooltipObject,
            AlgorithmType.GBFS => GBFSTooltipObject,
            AlgorithmType.Astar => AstarTooltipObject,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        _toolTipObject.SetActive(true);
    }
}
