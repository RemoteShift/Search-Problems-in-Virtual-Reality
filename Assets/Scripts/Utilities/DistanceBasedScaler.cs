using UnityEngine;

public class DistanceBasedScaler : MonoBehaviour
{
    [Header("Target (Camera or Player)")]
    [Tooltip("The transform to measure distance from. If null, will use the main camera.")]
    [SerializeField] private Transform target;

    [Header("Scaling Parameters")]
    [Tooltip("Distance at which scale = baseScale.")]
    [SerializeField] private float referenceDistance = 5f;

    [Tooltip("Scale at the reference distance.")]
    [SerializeField] private float baseScale = 1f;

    [Tooltip("Minimum allowed scale (prevents too-small labels).")]
    [SerializeField] private float minScale = 1f;

    [Tooltip("Maximum allowed scale (prevents gigantic labels).")]
    [SerializeField] private float maxScale = 3f;

    private Transform _cachedTransform;

    private void Start()
    {
        _cachedTransform = transform;
        if (!target)
            target = Camera.main?.transform;
        if (!target)
            Debug.LogWarning("DistanceBasedScaler: No target assigned and no MainCamera found.", this);
    }

    private void LateUpdate()
    {
        if (!target) return;

        var distance = Vector3.Distance(_cachedTransform.position, target.position);
        
        // Directly proportional scaling: farther = larger
        var rawScale = baseScale * (distance / referenceDistance);
        var scale = Mathf.Clamp(rawScale, minScale, maxScale);
        
        _cachedTransform.localScale = Vector3.one * scale;
    }
}