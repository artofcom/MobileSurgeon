using UnityEngine;
using Core.Events;

public class EventMessageTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnMessageTriggered(string key)
    {
        EventSystem.DispatchEvent("OnEventMessageTriggered", (object)key);
    }
}