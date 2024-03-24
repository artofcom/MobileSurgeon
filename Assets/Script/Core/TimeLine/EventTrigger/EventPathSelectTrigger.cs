using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using System;


[Serializable]
public class PreviewAnimInfo
{
    public Animator AnimObject;
    public string AniName;
}

[Serializable]
public class AnimPathDetailInfo
{
    public string DisplayName;
    public string DisplayHeader;
    public string PathKey;
    public int TimelineIndex;
    public long StartFrame;

    public PreviewAnimInfo[] PreviewAnimations;
}

[Serializable]
public class AnimPathInfo
{
    public string Key;
    public string MessageKey;
    public bool AllowDialogWhenDragging = false;
    public AnimPathDetailInfo[] PathOptions;
}


public class EventPathSelectTrigger : MonoBehaviour
{
    [SerializeField] List<AnimPathInfo> PathInfo;

    //  Events ----------------------------------------
    // EventsGroup Events = new EventsGroup();

    // Start is called before the first frame update
    void Start()
    { }

    private void OnDestroy()
    { }

    // Event Comes from Unity's TimeLine.
    public void OnTrigger(string key)
    {
        bool isValidKey = !string.IsNullOrEmpty(key);

        if (isValidKey)
        {
            AnimPathInfo targetInfo = null;
            for (int q = 0; q < PathInfo.Count; ++q)
            {
                if (PathInfo[q].Key == key)
                {
                    targetInfo = PathInfo[q];
                    break;
                }
            }

            Debug.Log($"Path Branch {key} Triggered.");
            if (targetInfo != null)
                EventSystem.DispatchEvent("EventPathSelectTrigger_OnTrigger", (object)targetInfo);
        }
        else
        {
            //Debug.Log("Inavlid Path Branch has Triggered.");
            EventSystem.DispatchEvent("EventPathSelectTrigger_OnTrigger", null);
        }
    }
}