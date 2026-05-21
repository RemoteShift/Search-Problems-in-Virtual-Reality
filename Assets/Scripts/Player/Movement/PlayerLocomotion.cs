using Search.Utils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : Singleton<PlayerLocomotion>
{
    public Transform tempQueueHandAttachmentPoint;
    public GameObject questionMarkCanvasObject;
    
    private CharacterController _characterController;
    private ContinuousMoveProvider _continuousMoveProvider;
    
    public float vMoveSpeed = 1.5f;
    private float _vUpInput;
    private float _vDownInput;
    
    public float sprintMultiplier = 2f;
    private bool _isSprinting;
    
    private float _initialMoveSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _continuousMoveProvider = GetComponent<ContinuousMoveProvider>();
        _initialMoveSpeed = _continuousMoveProvider.moveSpeed;
    }

    private void FixedUpdate()
    {
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

    public void ToggleSprint()
    {
        _isSprinting = !_isSprinting;
        _continuousMoveProvider.moveSpeed = _isSprinting ? _initialMoveSpeed * sprintMultiplier : _initialMoveSpeed;
    }

    public void SetSprintState(bool isSprinting)
    {
        _isSprinting = isSprinting;
        _continuousMoveProvider.moveSpeed = _isSprinting ? _initialMoveSpeed * sprintMultiplier : _initialMoveSpeed;
    }
    
    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        _characterController.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        _characterController.enabled = true;
    }

    public void ResetTransform()
    {
        _characterController.enabled = false;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        _characterController.enabled = true;
    }
}
