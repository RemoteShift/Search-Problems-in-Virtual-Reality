using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorController : MonoBehaviour
{
    [Header("Input Action References")]
    [SerializeField] private InputActionReference triggerValue;
    [SerializeField] private InputActionReference gripValue;

    private Animator handAnimator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        handAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var trigger = triggerValue.action.ReadValue<float>();
        var grip = gripValue.action.ReadValue<float>();
        
        handAnimator.SetFloat("Trigger", trigger);
        handAnimator.SetFloat("Grip", grip);
    }
}
