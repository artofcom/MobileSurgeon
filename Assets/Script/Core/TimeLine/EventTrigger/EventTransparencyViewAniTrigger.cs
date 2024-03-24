using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

public class EventTransparencyViewAniTrigger : MonoBehaviour
{
    [SerializeField] string IntroAniName;
    [SerializeField] string OuttroAniName;

    //  Events ----------------------------------------
    EventsGroup Events = new EventsGroup();

    bool mActivated = false;
    string mLastTriggeredAniName = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        Events.RegisterEvent("OnTransparencyViewClicked", OnTransparencyViewClicked);
    }

    // Event Comes from Unity's TimeLine.
    public void OnActivated(bool activated)
    {
        Debug.Log($"TimeLine EventTransparencyViewTrigger has activated! {activated}");
        
        EventSystem.DispatchEvent("EventTransparencyViewTrigger_OnActivated", (object)activated);

        if(mActivated && !activated && mLastTriggeredAniName!=OuttroAniName)
        {
            Animator animator = GetComponent<Animator>();
            animator.Play(OuttroAniName);
            mLastTriggeredAniName = OuttroAniName;
        }

        mActivated = activated;
    }

    // Event Comes from Button Click.
    void OnTransparencyViewClicked(object data)
    {
        if (!mActivated) return;

        bool ON = (bool)data;

        string strAniName = ON ? IntroAniName : OuttroAniName;
        Animator animator = GetComponent<Animator>();
        if (!string.IsNullOrEmpty(strAniName) && animator != null)
        {
            animator.Play(strAniName);
            mLastTriggeredAniName = strAniName;
        }
    }

    //  Private Functions --------------------------------
    //
    
}