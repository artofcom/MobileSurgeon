using UnityEngine;
using Core.Events;
using System;

[Serializable]
public class EventAniKeyInfo
{
    public string Key;
    public string IntroAniName;
    public string OuttroAniName;
}

public class EventAnimationTrigger : MonoBehaviour
{
    [SerializeField] EventAniKeyInfo[] AnimationInfo;

    //  Events ----------------------------------------
    EventsGroup Events = new EventsGroup();

    bool mActivated = false;
    string mCurAniKey = string.Empty;
    string mLastTriggeredAniName = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        Events.RegisterEvent("OnTransparencyViewClicked", OnTransparencyViewClicked);
    }
    private void OnDestroy()
    {
        Events.UnRegisterAll();
    }

    // Event Comes from Unity's TimeLine.
    public void OnActivated(string strAniKey)
    {
        bool activated = !string.IsNullOrEmpty(strAniKey);
        Debug.Log($"TimeLine EventTransparencyViewTrigger has activated! - {activated}");

        EventSystem.DispatchEvent("EventAnimationTrigger_OnActivated", (object)activated);

        var aniInfo = FindAniInfo(mCurAniKey);
        if (aniInfo != null && !activated && mLastTriggeredAniName != aniInfo.OuttroAniName)
        {
            // De-activating ? So we need to trigger the Outtro animation for this?
            //
            Animator animator = GetComponent<Animator>();
            animator.Play(aniInfo.OuttroAniName);
            mLastTriggeredAniName = aniInfo.OuttroAniName;
        }

        mCurAniKey = strAniKey;
        mActivated = activated;
    }

    // Event Comes from Button Click.
    void OnTransparencyViewClicked(object data)
    {
        if (!mActivated) return;

        bool ON = (bool)data;   // This is from a toggle button, so we can get both true and false.

        var aniInfo = FindAniInfo(mCurAniKey);
        if (aniInfo == null) return;

        string strAniName = ON ? aniInfo.IntroAniName : aniInfo.OuttroAniName;
        Animator animator = GetComponent<Animator>();
        if (!string.IsNullOrEmpty(strAniName) && animator != null)
        {
            animator.Play(strAniName);
            mLastTriggeredAniName = strAniName;
        }
    }

    //  Private Functions --------------------------------
    //
    EventAniKeyInfo FindAniInfo(string strKey)
    {
        if (AnimationInfo == null || AnimationInfo.Length == 0)
            return null;

        for (int k = 0; k < AnimationInfo.Length; ++k)
        {
            if (AnimationInfo[k].Key == strKey)
                return AnimationInfo[k];
        }
        return null;
    }
}