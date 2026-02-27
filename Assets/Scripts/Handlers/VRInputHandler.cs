using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class VRInputHandler : Singleton<VRInputHandler>
{
    [Header("Left Hand Inputs")]
    [SerializeField] private InputActionReference leftTriggerValue;
    [SerializeField] private InputActionReference leftGripValue;
    
    public float LeftTriggerValue { get; private set; }
    public float LeftGripValue { get; private set; }
    
    public event Action<float> OnLeftTriggerValueChanged;
    public event Action<float> OnLeftGripValueChanged;

    private void OnEnable()
    {
        leftTriggerValue.action.Enable();
        leftGripValue.action.Enable();
        
        leftTriggerValue.action.performed += HandleLeftTrigger;
        leftTriggerValue.action.canceled += HandleLeftTrigger;
        
        leftGripValue.action.performed += HandleLeftGrip;
        leftGripValue.action.canceled += HandleLeftGrip;
    }

    private void OnDisable()
    {
        leftTriggerValue.action.performed -= HandleLeftTrigger;
        leftTriggerValue.action.canceled -= HandleLeftTrigger;
        
        leftGripValue.action.performed -= HandleLeftGrip;
        leftGripValue.action.canceled -= HandleLeftGrip;
        
        leftTriggerValue.action.Disable();
        leftGripValue.action.Disable();
    }
    
    private void HandleLeftTrigger(InputAction.CallbackContext ctx)
    {
        LeftTriggerValue = ctx.ReadValue<float>();
        OnLeftTriggerValueChanged?.Invoke(LeftTriggerValue);
    }

    private void HandleLeftGrip(InputAction.CallbackContext ctx)
    {
        LeftGripValue = ctx.ReadValue<float>();
        OnLeftGripValueChanged?.Invoke(LeftGripValue);
    }
}
