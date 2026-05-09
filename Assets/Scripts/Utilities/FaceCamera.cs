using NaughtyAttributes;
using UnityEngine;

namespace Search.Utils
{
    public class FaceCamera : MonoBehaviour
    {
        [Tooltip("The camera to face. If null, will use the main camera.")]
        [SerializeField] private Camera playerCamera;

        [Foldout("Constraints")] public bool lockX;
        [Foldout("Constraints")] public bool lockY;
        [Foldout("Constraints")] public bool lockZ;

        public bool enable = true;
        
        private void Start()
        {
            playerCamera = !playerCamera ? Camera.main : playerCamera;
        }

        private void LateUpdate()
        {
            if (!playerCamera || !enable) return;
            
            var direction = transform.position - playerCamera.transform.position;
            
            if (direction == Vector3.zero) return;
            
            var targetRotation = Quaternion.LookRotation(direction);
            
            var currentEuler = transform.eulerAngles;
            var targetEuler = targetRotation.eulerAngles;
            
            if (lockX) targetEuler.x = currentEuler.x;
            if (lockY) targetEuler.y = currentEuler.y;
            if (lockZ) targetEuler.z = currentEuler.z;
            
            transform.rotation = Quaternion.Euler(targetEuler);
        }
    }
}
