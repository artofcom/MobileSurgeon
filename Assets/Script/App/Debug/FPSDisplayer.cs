using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core.Events;

namespace App.Debugger
{
    public class FPSDisplayer : MonoBehaviour
    {
        //  Events ----------------------------------------
        //
        EventsGroup Events = new EventsGroup();



        // [SerializeField] ------------------------------
        //
        [SerializeField] TMP_Text txtFPS;
        [SerializeField] int MaxFrames = 60;  //maximum frames to average over

        private static float lastFPSCalculated = 0f;
        private List<float> frameTimes = new List<float>();
        bool mDisplayEnabled = false;

        // [Unity Events] ------------------------------
        //
        // Start is called before the first frame update
        void Start()
        {
            lastFPSCalculated = 0f;
            frameTimes.Clear();

            if (txtFPS != null)
                txtFPS.gameObject.SetActive(mDisplayEnabled);

            Events.RegisterEvent("OptionTabView_OnDebugDisplayModeChanged", OptionTabView_OnDebugDisplayModeChanged);
        }

        // Update is called once per frame
        void Update()
        {
            addFrame();
            lastFPSCalculated = calculateFPS();

            if (txtFPS != null && mDisplayEnabled)
            {
                txtFPS.text = "FPS " + lastFPSCalculated;
            }

        }



        //  Events ----------------------------------------
        //
        void OptionTabView_OnDebugDisplayModeChanged(object data)
        {
            mDisplayEnabled = ((int)data) == 1;
            if (txtFPS != null)
                txtFPS.gameObject.SetActive(mDisplayEnabled);
        }


        // [Private Helpers] ------------------------------
        //
        private void addFrame()
        {
            frameTimes.Add(Time.unscaledDeltaTime);
            if (frameTimes.Count > MaxFrames)
            {
                frameTimes.RemoveAt(0);
            }
        }

        private float calculateFPS()
        {
            float newFPS = 0f;

            float totalTimeOfAllFrames = 0f;
            foreach (float frame in frameTimes)
            {
                totalTimeOfAllFrames += frame;
            }
            newFPS = ((float)(frameTimes.Count)) / totalTimeOfAllFrames;

            return newFPS;
        }

        public static float GetCurrentFPS()
        {
            return lastFPSCalculated;
        }
    }
}
