using UnityEngine;

public class ToggleSetActiveInverse : MonoBehaviour
{
    public void ToggleActive(bool value)
    {
        transform.GetChild(0).gameObject.SetActive(!value);
    }
}
