using UnityEngine;

public class PrintDestroyStack : MonoBehaviour
{
    private void OnDestroy()
    {
        var stackTrace = new System.Diagnostics.StackTrace();
        Debug.LogError($"Destroying {gameObject.name}. Stack trace:\n{stackTrace}");
    }
}
