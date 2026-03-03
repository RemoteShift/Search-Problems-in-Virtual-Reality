using System;
using UnityEngine;

public class RightControllerLocomotionInput : MonoBehaviour
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
        Debug.Log("Bruh");
        _inputHandler.OnRightTriggerValueChanged += HandleVUpInput;
        _inputHandler.OnRightGripValueChanged += HandleVDownInput;
    }

    private void OnDisable()
    {
        _inputHandler.OnRightTriggerValueChanged -= HandleVUpInput;
        _inputHandler.OnRightGripValueChanged -= HandleVDownInput;
        
        _playerLocomotion.SetVUpInput(0f);
        _playerLocomotion.SetVDownInput(0f);
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