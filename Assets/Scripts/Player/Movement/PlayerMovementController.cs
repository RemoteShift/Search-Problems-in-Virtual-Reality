using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class PlayerMovementController : MonoBehaviour
{
    private CharacterController _characterController;
    private ContinuousMoveProvider _continuousMoveProvider;
    
    public float vMoveSpeed = 1.5f;
    public float sprintMultiplier = 2f;
    
    private float _initialMoveSpeed;
    
    [Header("Input Action References")]
    [SerializeField] private InputActionReference leftTriggerValue;
    [SerializeField] private InputActionReference leftGripValue;
    [SerializeField] private InputActionReference leftScaleToggle;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _continuousMoveProvider = GetComponent<ContinuousMoveProvider>();
        _initialMoveSpeed = _continuousMoveProvider.moveSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var trigger = leftTriggerValue.action.ReadValue<float>();
        var grip = leftGripValue.action.ReadValue<float>();

        if (leftScaleToggle.action.IsPressed())
        {
            _continuousMoveProvider.moveSpeed =  _initialMoveSpeed * sprintMultiplier;
        }
        else
        {
            _continuousMoveProvider.moveSpeed = _initialMoveSpeed;
        }
        
        var moveDir = new Vector3(0, trigger - grip , 0);

        _characterController.Move(moveDir * (vMoveSpeed * Time.fixedDeltaTime));
    }
}
