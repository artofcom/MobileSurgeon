using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using System;

[Serializable]
public class TransparencyTargetAniInfo
{
    public Animator AnimatorTarget;
    public string AniTransitIn;
    public string AniTransitOut;
}

[Serializable]
public class TransparencyTargetInfo
{
    public string Key = string.Empty;
    public bool bDefaultPlay = false;
    public float DefaultPlayDuration = .0f;
    public List<GameObject> ActivaionTargets;               // Activation Turn On/Off.
    public List<TransparencyTargetAniInfo> AnimatorTargets; // Animation Trigger.
}

public class EventTransparencyGroupTrigger : MonoBehaviour
{
    [SerializeField] List<TransparencyTargetInfo> TargetsInfo;

    //  Events ----------------------------------------
    EventsGroup Events = new EventsGroup();

    bool mActivated = false;
    int mCurTargetIndex = -1;
    Coroutine mCoDefaultPlayer = null; 

    // Start is called before the first frame update
    void Start()
    {
        Events.RegisterEvent("OnTransparencyViewClicked", OnTransparencyViewClicked);
        Events.RegisterEvent("OnTransparencyViewDefaultPlay", OnTransparencyViewDefaultPlay);
    }
    private void OnDestroy()
    {
        Events.UnRegisterAll();
    }

    // Event Comes from Unity's TimeLine.
    public void OnActivated(string key)
    {
        mActivated = !string.IsNullOrEmpty(key);

        // Debug.Log($"TimeLine EventTransparencyViewTrigger has activated! {activated}");
        
        if (!mActivated)
        {
            TriggerAnimation(false);

            // Cache this AFTER trigger transitOut Ani.
            mCurTargetIndex = -1;

            ShowObjects(false);
        }
        else
        {
            mCurTargetIndex = FindTargetIndex(key);
        }

        EventSystem.DispatchEvent("EventTransparencyGroupTrigger_OnActivated", (object)key);
    }
    
    // Event Comes from Button Click.
    void OnTransparencyViewClicked(object data)
    {
        if (!mActivated) return;

        if (mCoDefaultPlayer != null)
            StopCoroutine(mCoDefaultPlayer);

        bool ON = (bool)data;

        ShowObjects(ON);
        TriggerAnimation(ON);
    }
    void OnTransparencyViewDefaultPlay(object data)
    {
        bool ON = (bool)data;
        var cur = mCurTargetIndex >= 0 ? TargetsInfo[mCurTargetIndex] : null;
        if (cur==null || cur.bDefaultPlay != ON)
            return;

        ShowObjects(ON);
        TriggerAnimation(ON);

        if (cur.DefaultPlayDuration > .0f)
        {
            if (mCoDefaultPlayer != null)
                StopCoroutine(mCoDefaultPlayer);

           mCoDefaultPlayer =
               StartCoroutine(coActionWithDelay(cur.DefaultPlayDuration, () =>
               {
                   OnTransparencyViewClicked(!ON);
                   mCoDefaultPlayer = null;
               }));
        }
    }

    //  Private Functions --------------------------------
    //
    IEnumerator coActionWithDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        if (action != null)
            action.Invoke();
    }
    void ShowObjects(bool bShow)
    {
        // Turn Off first.
        for (int k = 0; k < TargetsInfo.Count; ++k)
        {
            for (int q = 0; q < TargetsInfo[k].ActivaionTargets.Count; ++q)
                TargetsInfo[k].ActivaionTargets[q].SetActive(false);
        }

        var cur = mCurTargetIndex >= 0 ? TargetsInfo[mCurTargetIndex] : null;
        if (cur == null)
            return;


        // Only turn on the stuff.
        if (bShow)
        {
            for (int q = 0; q < cur.ActivaionTargets.Count; ++q)
                cur.ActivaionTargets[q].SetActive(true);
        }
    }
    void TriggerAnimation(bool bShow)
    {
        var cur = mCurTargetIndex >= 0 ? TargetsInfo[mCurTargetIndex] : null;
        if (cur == null)
            return;
        
        for (int q = 0; q < cur.AnimatorTargets.Count; ++q)
        {
            string animationName = bShow ? cur.AnimatorTargets[q].AniTransitIn : cur.AnimatorTargets[q].AniTransitOut;
            cur.AnimatorTargets[q].AnimatorTarget.Play(animationName);
        }
    }
    int FindTargetIndex(string key)
    {
        for (int k = 0; k < TargetsInfo.Count; ++k)
        {
            if (TargetsInfo[k].Key == key)
                return k;
        }
        return -1;
    }
}