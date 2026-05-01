using UnityEngine;

namespace Search.Culling
{
    [RequireComponent(typeof(Canvas))]
    public class CanvasCulling : MonoBehaviour
    {
        [Tooltip("The camera to measure visibility from. If left empty, the main camera will be used.")]
        [SerializeField]
        private Camera playerCamera;

        [Tooltip(
            "If true, Screen Space - Overlay canvases will be treated as always visible and the component " +
            "will disable itself.")]
        [SerializeField]
        private bool treatOverlayAsVisible = true;

        [Tooltip("Optional padding to expand the canvas AABB before frustum testing (world units).")] [SerializeField]
        private Vector3 aabbPadding = Vector3.zero;

        [Tooltip("Enable distance-based culling in addition to frustum checks.")] [SerializeField]
        private bool enableDistanceCulling = true;

        [Tooltip(
            "Maximum distance (world units) for distance culling. Public so legacy components can forward " +
            "their value.")]
        public float maxDistance = 10f;

        private Canvas _targetCanvas;
        private RectTransform _rectTransform;
        private readonly Plane[] _frustumPlanes = new Plane[6];
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private Camera _camera;

        private void Start()
        {
            playerCamera = playerCamera ? playerCamera : Camera.main;
            _targetCanvas = GetComponent<Canvas>();
            _rectTransform = _targetCanvas.GetComponent<RectTransform>();

            // If this is an overlay canvas, and we treat overlays as always visible, nothing to cull.
            if (_targetCanvas && _targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay &&
                treatOverlayAsVisible)
            {
                // Keep the canvas enabled and disable this component to save work.
                enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!_targetCanvas || !_rectTransform)
            {
                return;
            }

            // For overlay canvases, keep enabled
            if (_targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _targetCanvas.enabled = true;
                return;
            }

            // Distance culling (fast early-out)
            if (enableDistanceCulling)
            {
                var dist = Vector3.Distance(transform.position, playerCamera.transform.position);
                if (dist > maxDistance)
                {
                    _targetCanvas.enabled = false;
                    return;
                }
            }

            // Get world-space corners of the RectTransform
            _rectTransform.GetWorldCorners(_worldCorners);

            // Build an AABB from the corners
            var bounds = new Bounds(_worldCorners[0], Vector3.zero);
            for (var i = 1; i < _worldCorners.Length; i++)
                bounds.Encapsulate(_worldCorners[i]);

            if (aabbPadding != Vector3.zero)
            {
                bounds.Expand(aabbPadding);
            }

            GeometryUtility.CalculateFrustumPlanes(playerCamera, _frustumPlanes);
            var visible = GeometryUtility.TestPlanesAABB(_frustumPlanes, bounds);

            _targetCanvas.enabled = visible;
        }
    }
}