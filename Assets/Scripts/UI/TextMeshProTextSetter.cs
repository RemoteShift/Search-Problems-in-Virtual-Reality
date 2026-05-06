using TMPro;
using UnityEngine;

public class TextMeshProTextSetter : MonoBehaviour
{
    [SerializeField] private string textToSet = "Default Text";

    public string initialText;
    
    private TextMeshProUGUI _textMeshPro;

    private void Start()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        initialText = _textMeshPro.text;
    }
    
    public void ToggleText(bool value)
    {
        _textMeshPro.text = value ? textToSet : initialText;
    }
}
