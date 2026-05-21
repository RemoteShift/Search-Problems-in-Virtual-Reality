using UnityEngine;
using UnityEngine.EventSystems;

public class UIHasToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject toolTipObject;

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
        toolTipObject.SetActive(true);
    }
}
