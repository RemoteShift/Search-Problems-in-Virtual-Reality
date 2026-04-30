using UnityEngine;

public class DistanceCulling : MonoBehaviour
{
    private Canvas _targetCanvas;
    public float maxDistance = 10f;
    [Tooltip("The camera to measure distance from. If left empty, the main camera will be used.")]
    [SerializeField] private Camera playerCamera;

    private void Start()
    {
        playerCamera = !playerCamera ? Camera.main : playerCamera;
        _targetCanvas = GetComponent<Canvas>();
    }

    private void LateUpdate()
    {
        var dist = Vector3.Distance(transform.position, playerCamera.transform.position);
        _targetCanvas.enabled = dist < maxDistance;
    }
}
