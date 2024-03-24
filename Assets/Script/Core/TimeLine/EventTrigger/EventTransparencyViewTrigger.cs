using System.Collections.Generic;
using UnityEngine;
using Core.Events;

public class EventTransparencyViewTrigger : MonoBehaviour
{
    [SerializeField] GameObject ActivationRoot;
    [SerializeField] List<GameObject> ActivationTargets;

    //  Events ----------------------------------------
    EventsGroup Events = new EventsGroup();

    bool mActivated = false;

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
    public void OnActivated(bool activated)
    {
        Debug.Log($"TimeLine EventTransparencyViewTrigger has activated! {activated}");
        
        EventSystem.DispatchEvent("EventTransparencyViewTrigger_OnActivated", (object)activated);

        mActivated = activated;

        if (!activated)
            ShowObjects(false);
    }

    // Event Comes from Button Click.
    void OnTransparencyViewClicked(object data)
    {
        if (!mActivated) return;

        bool ON = (bool)data;
        ShowObjects(ON);
    }

    //  Private Functions --------------------------------
    //
    void ShowObjects(bool bShow)
    {
        if (ActivationRoot != null)
            ActivationRoot.SetActive(bShow);

        if (ActivationTargets != null && ActivationTargets.Count > 0)
        {
            for (int k = 0; k < ActivationTargets.Count; ++k)
                ActivationTargets[k].SetActive(bShow);
        }
    }
}