using Autohand;
using UnityEngine;

public class CanvasCameraAssigner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Canvas>().worldCamera = HandCanvasPointer.UICamera;
    }
}
