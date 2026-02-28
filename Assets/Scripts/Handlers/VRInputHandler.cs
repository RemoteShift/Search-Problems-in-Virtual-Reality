using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class VRInputHandler : Singleton<VRInputHandler>
{
    [Header("Left Hand Inputs")]
    [SerializeField] private InputActionReference leftTriggerValue;
    [SerializeField] private InputActionReference leftGripValue;
    [Header("Right Hand Inputs")]
    [SerializeField] private InputActionReference rightTriggerValue;
    [SerializeField] private InputActionReference rightGripValue;
    
    public float LeftTriggerValue { get; private set; }
    public float LeftGripValue { get; private set; }
    public float RightTriggerValue { get; private set; }
    public float RightGripValue { get; private set; }
    
    public event Action<float> OnLeftTriggerValueChanged;
    public event Action<float> OnLeftGripValueChanged;
    public event Action<float> OnRightTriggerValueChanged;
    public event Action<float> OnRightGripValueChanged;

    private void OnEnable()
    {
        leftTriggerValue.action.Enable();
        leftGripValue.action.Enable();
        rightTriggerValue.action.Enable();
        rightGripValue.action.Enable();
        
        leftTriggerValue.action.performed += HandleLeftTrigger;
        leftTriggerValue.action.canceled += HandleLeftTrigger;
        rightTriggerValue.action.performed += HandleRightTrigger;
        rightTriggerValue.action.canceled += HandleRightTrigger;
        
        leftGripValue.action.performed += HandleLeftGrip;
        leftGripValue.action.canceled += HandleLeftGrip;
        rightGripValue.action.performed += HandleRightGrip;
        rightGripValue.action.canceled += HandleRightGrip;
    }

    private void OnDisable()
    {
        leftTriggerValue.action.performed -= HandleLeftTrigger;
        leftTriggerValue.action.canceled -= HandleLeftTrigger;
        rightTriggerValue.action.performed -= HandleRightTrigger;
        rightTriggerValue.action.canceled -= HandleRightTrigger;
        
        leftGripValue.action.performed -= HandleLeftGrip;
        leftGripValue.action.canceled -= HandleLeftGrip;
        rightGripValue.action.performed -= HandleRightGrip;
        rightGripValue.action.canceled -= HandleRightGrip;
        
        leftTriggerValue.action.Disable();
        leftGripValue.action.Disable();
        rightTriggerValue.action.Disable();
        rightGripValue.action.Disable();
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
    
    private void HandleRightTrigger(InputAction.CallbackContext ctx)
    {
        RightTriggerValue = ctx.ReadValue<float>();
        OnRightTriggerValueChanged?.Invoke(RightTriggerValue);
    }
    
    private void HandleRightGrip(InputAction.CallbackContext ctx)
    {
        RightGripValue = ctx.ReadValue<float>();
        OnRightGripValueChanged?.Invoke(RightGripValue);
    }
}
