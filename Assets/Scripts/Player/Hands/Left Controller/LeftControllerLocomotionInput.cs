using System;
using UnityEngine;

public class LeftControllerLocomotionInput : MonoBehaviour
{
    private VRInputHandler _inputHandler;
    private PlayerLocomotion _playerLocomotion;

    private void Awake()
    {
        _inputHandler = VRInputHandler.Instance;
        _playerLocomotion = PlayerLocomotion.Instance;
    }

    private void OnEnable()
    {
        _inputHandler.OnLeftTriggerValueChanged += HandleVUpInput;
        _inputHandler.OnLeftGripValueChanged += HandleVDownInput;
        _inputHandler.OnLeftScalePressChanged += HandleScalePressChanged;
        _inputHandler.OnLeftMoveValueChanged += HandleMoveValueChanged;
    }

    private void OnDisable()
    {
        _inputHandler.OnLeftTriggerValueChanged -= HandleVUpInput;
        _inputHandler.OnLeftGripValueChanged -= HandleVDownInput;
        _inputHandler.OnLeftScalePressChanged -= HandleScalePressChanged;
        _inputHandler.OnLeftMoveValueChanged -= HandleMoveValueChanged;
        
        _playerLocomotion.SetVUpInput(0f);
        _playerLocomotion.SetVDownInput(0f);
        _playerLocomotion.SetSprintState(false);
    }

    private void HandleVUpInput(float value)
    {
        _playerLocomotion.SetVUpInput(value);
    }

    private void HandleVDownInput(float value)
    {
        _playerLocomotion.SetVDownInput(value);
    }
    
    private void HandleScalePressChanged(bool isPressed)
    {
        if (isPressed)
        {
            _playerLocomotion.ToggleSprint();
        }
    }
    
    private void HandleMoveValueChanged(Vector2 value)
    {
        if (value.magnitude < 0.1f)
        {
            _playerLocomotion.SetSprintState(false);
        }
    }
}