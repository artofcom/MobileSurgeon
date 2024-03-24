using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

public class EventTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnSectionStarted(int id)
    {
        Debug.Log($"TimeLine Event triggered! {id}");

        EventSystem.DispatchEvent("OnSectionStarted", (object)id);
    }
}