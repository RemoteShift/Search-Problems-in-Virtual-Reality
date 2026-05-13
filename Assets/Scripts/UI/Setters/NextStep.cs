using Search.Controllers;
using Search.GameModes;
using UnityEngine;

public class NextStep : MonoBehaviour
{
    private void Start()
    {
        var button = GetComponent<UnityEngine.UI.Button>();
        if (button)
        {
            button.onClick.AddListener(SearchModeController.Instance.PlayerRequestsAdvance);
        }
    }
}
