using System;
using UnityEngine;

public class QueueDropZone : MonoBehaviour
{
    [SerializeField] private PlayerQueueUI playerQueueUI;
    public int zoneIndex;

    public bool isHoveringDropZone { get; private set; }
    
    private bool _wasInProximity;
    private QueueElementUI currentlyGrabbedQueueElement => 
        PlayerQueueUI.Instance.currentlyGrabbedQueueElementObject.GetComponent<QueueElementUI>();

    // private void OnDestroy()
    // {
    //     Debug.Log($"Object {gameObject.name} destroyed. StackTrace: \n{StackTraceUtility.ExtractStackTrace()}");
    // }

    protected void Awake()
    {
        if (!playerQueueUI)
            playerQueueUI = PlayerQueueUI.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("QueueElement"))
            return;

        isHoveringDropZone = true;
        PlayerQueueUI.Instance.SetHoveringIndex(zoneIndex);
        currentlyGrabbedQueueElement.isDroppingIntoQueue = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("QueueElement"))
            return;

        isHoveringDropZone = false;
        PlayerQueueUI.Instance.SetHoveringIndex(-1);
        currentlyGrabbedQueueElement.isDroppingIntoQueue = false;
    }
}