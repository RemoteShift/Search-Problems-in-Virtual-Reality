using Search.Controllers;
using UnityEngine;

public class NextStep : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<UnityEngine.UI.Button>();
        if (button)
        {
            button.onClick.AddListener(SearchController.Instance.AdvanceStep);
        }
    }
}
