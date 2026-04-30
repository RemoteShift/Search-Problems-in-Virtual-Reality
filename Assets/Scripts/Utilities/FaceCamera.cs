using UnityEngine;

namespace Search.Utils
{
    public class FaceCamera : MonoBehaviour
    {
        [Tooltip("The camera to face. If null, will use the main camera.")]
        [SerializeField] private Camera playerCamera;

        private void Start()
        {
            playerCamera = !playerCamera ? Camera.main : playerCamera;
        }

        private void LateUpdate()
        {
            // Alternative: full 3D facing (like a nameplate)
            transform.LookAt(playerCamera.transform);
            //transform.Rotate(0, 180, 0);
        }
    }
}
