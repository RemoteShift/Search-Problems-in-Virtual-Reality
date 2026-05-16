using Search.GameModes;
using UnityEngine;
using UnityEngine.UI;

public class SetSolveModeUI : MonoBehaviour
{
    private void Start()
    {
        var toggle = GetComponent<Toggle>();

        if (toggle)
        {
            toggle.onValueChanged.RemoveAllListeners();
            toggle.isOn = SearchModeController.Instance.CurrentMode == SearchPlayMode.Solve;

            toggle.onValueChanged.AddListener((bool value) =>
            {
                SearchModeController.Instance.SetMode(value ? SearchPlayMode.Solve : SearchPlayMode.Observe);
            });
        }

        var dropDown = GetComponent<Dropdown>();

        if (dropDown)
        {
            dropDown.value =  SearchModeController.Instance.CurrentMode == SearchPlayMode.Solve ? 0 : 1;
        }
    }

    public void SetSolveMode(int mode)
    {
        SearchModeController.Instance.SetMode(mode == 0 ? SearchPlayMode.Solve : SearchPlayMode.Observe);
    }
}
