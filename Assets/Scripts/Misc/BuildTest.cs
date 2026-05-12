using UnityEngine;
using UnityEngine.EventSystems;

public class BuildTest : MonoBehaviour
{
    void Start()
    {
        var es = EventSystem.current;

        Debug.Log("EventSystem current is NULL? " + (es == null));

        if (es != null)
        {
            Debug.Log("EventSystem name: " + es.gameObject.name);
            Debug.Log("Input Module type: " +
                      (es.currentInputModule != null
                          ? es.currentInputModule.GetType().Name
                          : "NULL"));
        }
    }
}