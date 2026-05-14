using Search.GameModes;
using UnityEngine;
using UnityEngine.UI;

public class SetSolveModeUI : MonoBehaviour
{
    private void Start()
    {
        var toggle = GetComponent<Toggle>();
        toggle.onValueChanged.RemoveAllListeners();
        
        toggle.isOn = SearchModeController.Instance.CurrentMode == SearchPlayMode.Solve;
        
        toggle.onValueChanged.AddListener((bool value) =>
        {
            SearchModeController.Instance.SetMode(value ? SearchPlayMode.Solve : SearchPlayMode.Observe);
            
        });
    }
}
