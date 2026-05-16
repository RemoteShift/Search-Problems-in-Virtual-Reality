using UnityEngine;

public class QueueDropZone : MonoBehaviour
{
    [SerializeField] private PlayerQueueUI playerQueueUI;
    public int zoneIndex => transform.GetSiblingIndex();
    
    private bool _wasInProximity;

    protected void Awake()
    {
        if (!playerQueueUI)
            playerQueueUI = PlayerQueueUI.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("QueueElement"))
            return;
        
        PlayerQueueUI.Instance.RegisterHover(this);
        var grabbed = PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject;
        if (grabbed) grabbed.GetComponent<QueueElementUI>().isDroppingIntoQueue = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("QueueElement"))
            return;
        
        PlayerQueueUI.Instance.UnregisterHover(this);
        var grabbed = PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject;
        if (grabbed) grabbed.GetComponent<QueueElementUI>().isDroppingIntoQueue = false;
    }
}