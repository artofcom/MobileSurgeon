using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

namespace App.MVCS
{
    public class DebugScreenView : MonoBehaviour
    {
        [SerializeField] GameObject BtnTakeThumbnailShots;

        //  Events ----------------------------------------
        //
        EventsGroup Events = new EventsGroup();


        //bool mViewEnabled = false;
        //bool mIsAnimationViewEnabled = false;

        // Start is called before the first frame update
        void Start()
        {
            Events.RegisterEvent("OptionTabView_OnDebugDisplayModeChanged", OptionTabView_OnDebugDisplayModeChanged);

            Events.RegisterEvent("SurgeAnimationView_OnEnable", SurgeAnimationView_OnEnable);
            Events.RegisterEvent("SurgeAnimationView_OnDisable", SurgeAnimationView_OnDisable);

            Refresh();
        }





        public void OnTakeScreenShotBtnClicked()
        {
            EventSystem.DispatchEvent("DebugScreenView_OnTakeScreenShotBtnClicked");
        }
        void OptionTabView_OnDebugDisplayModeChanged(object data)
        {
            //mViewEnabled = ((int)data) == 1;
            Refresh();
        }
        void SurgeAnimationView_OnEnable(object data)
        {
            //mIsAnimationViewEnabled = true;
            Refresh();
        }
        void SurgeAnimationView_OnDisable(object data)
        {
            //mIsAnimationViewEnabled = false;
            Refresh();
        }

        void Refresh()
        {
            //bool isEditorMode = false;
#if UNITY_EDITOR
            //isEditorMode = true;
#endif
            BtnTakeThumbnailShots.gameObject.SetActive(false);// isEditorMode && mViewEnabled && mIsAnimationViewEnabled);
        }
    }
}