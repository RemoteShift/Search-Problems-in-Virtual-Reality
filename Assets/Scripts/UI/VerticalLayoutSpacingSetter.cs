using UnityEngine;
using UnityEngine.UI;

public class VerticalLayoutSpacingSetter : MonoBehaviour
{
    [SerializeField] private float spacing = 32f;

    private VerticalLayoutGroup _layoutGroup;
    
    private void Start()
    {
        _layoutGroup = GetComponent<VerticalLayoutGroup>();
    }

    public void ToggleSpacing(bool value)
    {
        _layoutGroup.spacing = value ? 0f : spacing;
    }
}
