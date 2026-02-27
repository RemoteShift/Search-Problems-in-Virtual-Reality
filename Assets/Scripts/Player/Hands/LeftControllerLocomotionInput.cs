using System;
using UnityEngine;

public class LeftHandLocomotionInput : MonoBehaviour
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
    }

    private void OnDisable()
    {
        _inputHandler.OnLeftTriggerValueChanged -= HandleVUpInput;
        _inputHandler.OnLeftGripValueChanged -= HandleVDownInput;
        
        _playerLocomotion.SetVUpInput(0f);
    }

    private void HandleVUpInput(float value)
    {
        _playerLocomotion.SetVUpInput(value);
    }

    private void HandleVDownInput(float value)
    {
        _playerLocomotion.SetVDownInput(value);
    }
}