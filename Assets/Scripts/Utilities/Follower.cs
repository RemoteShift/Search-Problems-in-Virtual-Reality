using UnityEngine;

namespace Search.Utils
{
    public class Follower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        public float followSpeed = 5f;
        public float rotationFollowSpeed = 5f;
        [SerializeField] private float yOffset = 1.2f;
        [Tooltip("Local offset relative to the target (in target's local space)")]
        [SerializeField] private Vector3 localOffset = Vector3.zero;

        private Vector3 _totalOffset;

        private void Start()
        {
            if (!target)
            {
                target = PlayerLocomotion.Instance.transform;
            }
        }

        private void LateUpdate()
        {
            _totalOffset = localOffset + Vector3.up * yOffset;
            var desiredPosition = target.TransformPoint(_totalOffset);
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                rotationFollowSpeed * Time.deltaTime
            );
        }
    }
}

