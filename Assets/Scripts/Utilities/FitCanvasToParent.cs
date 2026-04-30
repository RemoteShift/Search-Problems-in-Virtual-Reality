using System;
using UnityEngine;

namespace Search.Utils
{
    [RequireComponent(typeof(Canvas))]
    public class FitCanvasToParent : MonoBehaviour
    {
        public enum FitMode
        {
            WidthAndHeight,
            WidthOnly,
            HeightOnly
        }

        public FitMode fitMode = FitMode.WidthAndHeight;

        public bool useColliderBounds = true; // if true, use Collider; else use Renderer
        public Vector3 padding = Vector3.zero; // extra space around

        private void Start()
        {
            Resize();
        }

        public void Resize()
        {
            var parent = transform.parent;
            if (!parent)
            {
                Debug.LogWarning("Canvas has no parent to fit to.");
                return;
            }

            var bounds = GetParentBounds(parent);
            var rt = GetComponent<RectTransform>();
            if (!rt)
            {
                return;
            }

            var targetWidth = bounds.size.x + padding.x;
            var targetHeight = bounds.size.z + padding.z; // Z for vertical dimension

            switch (fitMode)
            {
                case FitMode.WidthAndHeight:
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
                    break;
                case FitMode.WidthOnly:
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                    break;
                case FitMode.HeightOnly:
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Bounds GetParentBounds(Transform parent)
        {
            var bounds = new Bounds(parent.position, Vector3.zero);
            var hasBounds = false;

            // Check parent's collider first
            if (useColliderBounds)
            {
                var col = parent.GetComponent<Collider>();
                if (col)
                {
                    bounds = col.bounds;
                    hasBounds = true;
                }
            }

            // If no collider or not using collider, check parent's renderer
            if (!hasBounds)
            {
                var rend = parent.GetComponent<Renderer>();
                if (rend)
                {
                    bounds = rend.bounds;
                    hasBounds = true;
                }
            }

            // Fallback: if still nothing, search children? (optional)
            // If you don't want child fallback, just return default bounds.
            if (!hasBounds)
            {
                Debug.LogWarning($"No Collider or Renderer found on {parent.name}. Using local scale.");
                bounds.size = parent.localScale;
            }

            return bounds;
        }
    }
}