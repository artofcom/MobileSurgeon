using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using System;

[Serializable]
public class ShotInfo
{
    public Sprite Shot;
    [TextArea(3, 10)]
    public string Name; // AP, Mortise, Lateral Etc...
}

[Serializable]
public class XRayData
{
    public string key;
    public ShotInfo[] ShotInfos;
}

public class EventXRayViewTrigger : MonoBehaviour
{

    [SerializeField] List<XRayData> XRayData;

    //  Events ----------------------------------------
    //EventsGroup Events = new EventsGroup();

    XRayData mCurData = null;


    // Start is called before the first frame update
    void Start()
    { }


    // Event Comes from Unity's TimeLine.
    public void OnActivated(string key)
    {
        Debug.Log($"TimeLine EventXRayViewTrigger has activated! {key}");

        mCurData = GetData(key);
        EventSystem.DispatchEvent("EventXRayViewTrigger_OnActivated", (object)mCurData);
        EventSystem.DispatchEvent("EventXRayViewTrigger_OnMessageActivated", (object)key);
    }




    //  Private Functions --------------------------------
    //
    XRayData GetData(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        key = key.ToLower();
        for (int k = 0; k < XRayData.Count; ++k)
        {
            if (XRayData[k].key.ToLower() == key)
                return XRayData[k];
        }
        return null;
    }

}