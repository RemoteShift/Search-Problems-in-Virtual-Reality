using UnityEngine;

public class LeftControllerUI : MonoBehaviour
{
    private VRInputHandler _inputHandler;
    
    [SerializeField] private GameObject controllerCanvas;

    private void Awake()
    {
        _inputHandler = VRInputHandler.Instance;
    }
    
    private void OnEnable()
    {
        _inputHandler.OnLeftMenuPressChanged += HandleUIToggle;
    }
    
    private void OnDisable()
    {
        _inputHandler.OnLeftMenuPressChanged -= HandleUIToggle;
        controllerCanvas.SetActive(false);
    }

    private void HandleUIToggle(bool isPressed)
    {
        if(isPressed)
            controllerCanvas.SetActive(!controllerCanvas.activeSelf);
    }
}
