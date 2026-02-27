using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : Singleton<PlayerLocomotion>
{
    private CharacterController _characterController;
    private ContinuousMoveProvider _continuousMoveProvider;
    
    public float vMoveSpeed = 1.5f;
    private float _vUpInput;
    private float _vDownInput;
    
    public float sprintMultiplier = 2f;
    
    private float _initialMoveSpeed;
    
    [Header("Input Action References")]
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
        if (leftScaleToggle.action.IsPressed())
        {
            _continuousMoveProvider.moveSpeed =  _initialMoveSpeed * sprintMultiplier;
        }
        else
        {
            _continuousMoveProvider.moveSpeed = _initialMoveSpeed;
        }
        
        var moveDir = new Vector3(0, _vUpInput - _vDownInput , 0);

        _characterController.Move(moveDir * (vMoveSpeed * Time.fixedDeltaTime));
    }
    
    public void SetVUpInput(float value)
    {
        _vUpInput = value;
    }

    public void SetVDownInput(float value)
    {
        _vDownInput = value;
    }
}
