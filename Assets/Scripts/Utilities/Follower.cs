using UnityEngine;
using UnityEngine.Serialization;

public class Follower : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    public float followSpeed = 5f;

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPosition = target.position;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}
