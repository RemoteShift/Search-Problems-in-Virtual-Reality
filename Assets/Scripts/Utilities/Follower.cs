using UnityEngine;

namespace Search.Utils
{
    public class Follower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        public float followSpeed = 5f;
        public float rotationFollowSpeed = 5f;

        // Update is called once per frame
        void Update()
        {
            Vector3 desiredPosition = target.position;
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
