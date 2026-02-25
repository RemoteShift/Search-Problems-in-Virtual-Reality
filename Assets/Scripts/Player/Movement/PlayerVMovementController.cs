using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVMovementController : MonoBehaviour
{
    private CharacterController _characterController;
    
    public float moveSpeed = 1f;
    
    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference leftTriggerValue;
    [SerializeField] private InputActionReference leftGripValue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var trigger = leftTriggerValue.action.ReadValue<float>();
        var grip = leftGripValue.action.ReadValue<float>();
        
        var moveDir = new Vector3(0, trigger - grip , 0);

        _characterController.Move(moveDir * (moveSpeed * Time.fixedDeltaTime));
    }
}
