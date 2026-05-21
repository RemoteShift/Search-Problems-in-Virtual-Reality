using UnityEngine;

public class DistanceGrabbableHasToolTip : MonoBehaviour
{
    [SerializeField] private GameObject toolTipObject;
    [SerializeField] private GameObject questionMarkCanvasObject;

    public void SubscribeShowToolTip(bool isHovering)
    {
        if (isHovering)
        {
            VRInputHandler.Instance.OnRightGripAndAValueChanged += ShowToolTip;
            questionMarkCanvasObject.SetActive(true);
        }
        else
        {
            VRInputHandler.Instance.OnRightGripAndAValueChanged -= ShowToolTip;
            questionMarkCanvasObject.SetActive(false);
        }
    }
    
    private void ShowToolTip()
    {
        toolTipObject.SetActive(true);
    }
}
