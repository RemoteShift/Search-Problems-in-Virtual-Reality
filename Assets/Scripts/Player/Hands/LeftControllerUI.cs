using UnityEngine;

public class LeftControllerUI : MonoBehaviour
{
    private VRInputHandler _inputHandler;

    public bool isUIActive;
    
    public GameObject controllerCanvas;
    [SerializeField] private Rigidbody rb;
    private Vector3 _initialCanvasPosition;

    private void Awake()
    {
        _inputHandler = VRInputHandler.Instance;
        if (controllerCanvas)
        {
            _initialCanvasPosition = controllerCanvas.transform.localPosition;
        }
    }
    
    private void OnEnable()
    {
        _inputHandler.OnLeftMenuPressChanged += HandleUIToggle;
    }
    
    private void OnDisable()
    {
        _inputHandler.OnLeftMenuPressChanged -= HandleUIToggle;
        if(controllerCanvas)
            controllerCanvas.SetActive(false);
    }

    private void HandleUIToggle(bool isPressed)
    {
        if (!controllerCanvas || !isUIActive)
            return;
        
        if (isPressed)
        {
            controllerCanvas.SetActive(!controllerCanvas.activeSelf);
            if (controllerCanvas.activeSelf)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                controllerCanvas.transform.SetParent(transform);
                controllerCanvas.transform.SetLocalPositionAndRotation(_initialCanvasPosition, Quaternion.Euler(22, -84, 2));
            }
        }
    }
}
