using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class RightToLeftScrollRect : MonoBehaviour
{
    private ScrollRect _scrollRect;
    [SerializeField] private Scrollbar targetScrollbar;

    private void Start()
    {
        _scrollRect = GetComponent<ScrollRect>();
        if (!targetScrollbar) // Auto-find the horizontal scrollbar if not manually set
        {
            targetScrollbar = _scrollRect.horizontalScrollbar;
        }

        if (targetScrollbar)
        {
            // Initialize: Set content to the far right when scrollbar = 0
            _scrollRect.horizontalNormalizedPosition = 1f - targetScrollbar.value;

            // Create a persistent link: Invert the value every time the scrollbar moves
            targetScrollbar.onValueChanged.AddListener((value) => {
                _scrollRect.horizontalNormalizedPosition = 1f - value;
            });
        }
        else
        {
            Debug.LogError("RightToLeftScrollRect: Could not find a horizontal scrollbar!", this);
        }
    }
}