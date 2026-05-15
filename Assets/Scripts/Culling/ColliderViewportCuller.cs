using UnityEngine;
using UnityEngine.UI;

public class ColliderViewportCuller : MonoBehaviour
{
    private Collider _targetCollider3D;
    private RectTransform _rectTransform;
    private ScrollRect _parentScrollRect;
    private RectTransform _viewportRect;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _targetCollider3D = GetComponent<Collider>();

        // Automatically finds the ScrollRect this item lives inside of
        _parentScrollRect = GetComponentInParent<ScrollRect>();

        if (_parentScrollRect)
        {
            _viewportRect = _parentScrollRect.viewport;
            // Listen to scroll movements
            _parentScrollRect.onValueChanged.AddListener(OnScrollUpdate);
            
            OnScrollUpdate(Vector2.zero); // Initial cull check
        }
    }

    private void OnDestroy()
    {
        if (_parentScrollRect)
        {
            _parentScrollRect.onValueChanged.RemoveListener(OnScrollUpdate);
        }
    }

    private void OnScrollUpdate(Vector2 scrollPos)
    {
        if (!_viewportRect)
        {
            return;
        }

        // 1. Get viewport bounding corners in world coordinates
        var viewCorners = new Vector3[4];
        _viewportRect.GetWorldCorners(viewCorners);
        var viewBounds = new Rect(viewCorners[0].x, viewCorners[0].y, viewCorners[2].x - viewCorners[0].x,
            viewCorners[2].y - viewCorners[0].y);

        // 2. Get this specific item's bounding corners in world coordinates
        var itemCorners = new Vector3[4];
        _rectTransform.GetWorldCorners(itemCorners);
        var itemBounds = new Rect(itemCorners[0].x, itemCorners[0].y, itemCorners[2].x - itemCorners[0].x,
            itemCorners[2].y - itemCorners[0].y);

        // 3. Determine if the item overlaps the visible viewport mask
        var isVisible = viewBounds.Overlaps(itemBounds);

        if (_targetCollider3D && _targetCollider3D.enabled != isVisible)
        {
            _targetCollider3D.enabled = isVisible;
        }
    }
}