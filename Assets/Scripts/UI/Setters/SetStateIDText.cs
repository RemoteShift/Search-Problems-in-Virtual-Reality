using UnityEngine;
using Search.Visualization;
using TMPro;

namespace Search.UI
{
    public class SetStateIDText : MonoBehaviour
    {
        private NodeVisual _nodeVisual;
        private TextMeshProUGUI _textMeshPro;

        private void Start()
        {
            _nodeVisual = transform.parent.parent.GetComponent<NodeVisual>();
            _textMeshPro = GetComponent<TextMeshProUGUI>();

            if (_nodeVisual && _textMeshPro)
            {
                _textMeshPro.text = _nodeVisual.stateId;
            }
        }
    }
}
