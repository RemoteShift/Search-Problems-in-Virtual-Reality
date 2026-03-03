using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Numerics;
using UnityEngine.Serialization;
using Vector2 = UnityEngine.Vector2;

public class VRInputHandler : Singleton<VRInputHandler>
{
    [Header("Left Hand Inputs")]
    [SerializeField] private InputActionReference leftTriggerValue;
    [SerializeField] private InputActionReference leftGripValue;
    [SerializeField] private InputActionReference leftScalePress;
    [SerializeField] private InputActionReference leftMoveValue;
    [SerializeField] private InputActionReference leftUIPress;
    [Header("Right Hand Inputs")]
    [SerializeField] private InputActionReference rightTriggerValue;
    [SerializeField] private InputActionReference rightGripValue;
    
    public event Action<float> OnLeftTriggerValueChanged;
    public event Action<float> OnLeftGripValueChanged;
    public event Action<bool> OnLeftScalePressChanged;
    public event Action<Vector2> OnLeftMoveValueChanged;
    public event Action<bool> OnLeftUIPressChanged;
    public event Action<float> OnRightTriggerValueChanged;
    public event Action<float> OnRightGripValueChanged;

    private void OnEnable()
    {
        leftTriggerValue.action.Enable();
        leftGripValue.action.Enable();
        leftScalePress.action.Enable();
        leftMoveValue.action.Enable();
        leftUIPress.action.Enable();
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

        leftScalePress.action.performed += HandleLeftScalePress;
        leftScalePress.action.canceled += HandleLeftScalePress;
        
        leftMoveValue.action.performed += HandleLeftMoveValue;
        leftMoveValue.action.canceled += HandleLeftMoveValue;

        leftUIPress.action.performed += HandleLeftUIPress;
        leftUIPress.action.canceled += HandleLeftUIPress;
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
        
        leftScalePress.action.performed -= HandleLeftScalePress;
        leftScalePress.action.canceled -= HandleLeftScalePress;
        
        leftMoveValue.action.performed -= HandleLeftMoveValue;
        leftMoveValue.action.canceled -= HandleLeftMoveValue;

        leftUIPress.action.performed -= HandleLeftUIPress;
        leftUIPress.action.canceled -= HandleLeftUIPress;
        
        leftTriggerValue.action.Disable();
        leftGripValue.action.Disable();
        leftScalePress.action.Disable();
        leftMoveValue.action.Disable();
        leftUIPress.action.Disable();
        rightTriggerValue.action.Disable();
        rightGripValue.action.Disable();
    }
    
    private void HandleLeftTrigger(InputAction.CallbackContext ctx)
    {
        OnLeftTriggerValueChanged?.Invoke(ctx.ReadValue<float>());
    }

    private void HandleLeftGrip(InputAction.CallbackContext ctx)
    {
        OnLeftGripValueChanged?.Invoke(ctx.ReadValue<float>());
    }
    
    private void HandleLeftScalePress(InputAction.CallbackContext ctx)
    {
        OnLeftScalePressChanged?.Invoke(ctx.ReadValueAsButton());
    }
    
    private void HandleLeftMoveValue(InputAction.CallbackContext ctx)
    {
        OnLeftMoveValueChanged?.Invoke(ctx.ReadValue<Vector2>());
    }
    
    private void HandleLeftUIPress(InputAction.CallbackContext ctx)
    {
        OnLeftUIPressChanged?.Invoke(ctx.ReadValueAsButton());
    }
    
    private void HandleRightTrigger(InputAction.CallbackContext ctx)
    {
        OnRightTriggerValueChanged?.Invoke(ctx.ReadValue<float>());
    }
    
    private void HandleRightGrip(InputAction.CallbackContext ctx)
    {
        OnRightGripValueChanged?.Invoke(ctx.ReadValue<float>());
    }
}
