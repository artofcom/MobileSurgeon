using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.UI;

namespace App.MVCS
    {
    public class SurgeAnimationView : AView, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] GameObject Scene3DObject;
        [SerializeField] GameObject WelcomeObject;
        [SerializeField] SurgeTopUIView TopUIView;
        [SerializeField] SurgeAnimation3DView Ani3DView;
        [SerializeField] float WelcomeViewDuration = 3.0f;

        // Intro View.
        [SerializeField] TMP_Text IntroText;

        [SerializeField] GameObject ExpertRoot;
        [SerializeField] GameObject BeginnerRoot;

        // Beginner Mode Setup
        [SerializeField] GameObject BtnPlayNextSector;

        // Expert Mode Setup
        [SerializeField] RectTransform TrBottomArea;
        [SerializeField] RectTransform TrUpPos, TrDownPos;
        [SerializeField] GameObject BtnShowMsg, BtnHideMsg;
        [SerializeField] GameObject ImgMessageBG;
        [SerializeField] GameObject MessageScrollBox;

        // Utility View.
        [SerializeField] GameObject BtnTrasparencyView, BtnXRayView;
        [SerializeField] TMP_Text BtnTransparencyText;
        [SerializeField] GameObject LoadingObject;

        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        public SurgeTopUIView SurgeTopUIView => TopUIView;
        public SurgeAnimation3DView SurgeAniView => Ani3DView;

        bool mTransparencyViewON = false;

        //  Unity Event Handlers ------------------------------
        // Start is called before the first frame update
        void Start()
        {
            Events.RegisterEvent("EventTransparencyViewTrigger_OnActivated", EventTransparencyViewTrigger_OnActivated);
            Events.RegisterEvent("EventTransparencyGroupTrigger_OnActivated", EventTransparencyGroupTrigger_OnActivated);

            Events.RegisterEvent("EventXRayViewTrigger_OnActivated", EventXRayViewTrigger_OnActivated);
            Events.RegisterEvent("EventAnimationTrigger_OnActivated", EventAnimationTrigger_OnActivated);
        }
        private void OnEnable()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeAnimationView_OnEnable");
            mTransparencyViewON = false;
        }
        private void OnDisable()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeAnimationView_OnDisable");
            LoadingObject.SetActive(false);
        }
        private void OnApplicationQuit()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeAnimationView_OnApplicationQuit");
        }


        //  Methoes --------------------------------------------
        public void StartAnimationView(bool isExpertMode, string titleMsg)
        {
            ExpertRoot.SetActive(isExpertMode);
            BeginnerRoot.SetActive(!isExpertMode);

            if (isExpertMode)
            {
                BtnShowMsg.SetActive(false);
                BtnHideMsg.SetActive(true);
            }

            BtnPlayNextSector.SetActive(false);

            Scene3DObject.SetActive(true);
            WelcomeObject.SetActive(true);
            IntroText.text = titleMsg;
            TrBottomArea.localPosition = TrUpPos.localPosition;
            ImgMessageBG.SetActive(true);
            MessageScrollBox.SetActive(true);

            BtnXRayView.GetComponent<Button>().interactable = false;
            BtnTrasparencyView.GetComponent<Button>().interactable = false;

            StartCoroutine(coFadeOutWelcomeView());
        }
        public void LeaveAnimationView()
        {
            Scene3DObject.SetActive(false);
        }
        IEnumerator coFadeOutWelcomeView()
        {
            yield return new WaitForSeconds(WelcomeViewDuration);
            WelcomeObject.SetActive(false);
        }
        public void OnPaused()
        {
            BtnPlayNextSector.SetActive(true);
        }
        public void OnPlayNextSectorClicked()
        {
            BtnPlayNextSector.SetActive(false);
        }

        // Delivery Only.
        public void TriggerXRayMessage(string message)
        {
            TopUIView.TriggerXRayMessage(message);
        }
        public void TriggerTransparencyMessage(string message)
        {
            TopUIView.TriggerTransparencyMessage(message);
        }


        //  Event Handlers --------------------------------
        public void OnHideMessageClicked()
        {
            BtnShowMsg.SetActive(true);
            BtnHideMsg.SetActive(false);
            // TrBottomArea.localPosition = new Vector3(.0f, TrDownPos.localPosition.y, .0f);
            StartCoroutine(CoMoveTo(TrBottomArea, TrDownPos.localPosition, 0.1f, () =>
            {
                ImgMessageBG.SetActive(false);
                MessageScrollBox.SetActive(false);
            }));
        }

        public void OnShowMessageClicked()
        {
            BtnShowMsg.SetActive(false);
            BtnHideMsg.SetActive(true);
            // TrBottomArea.localPosition = new Vector3(.0f, TrUpPos.localPosition.y, .0f);
            ImgMessageBG.SetActive(true);
            MessageScrollBox.SetActive(true);
            StartCoroutine(CoMoveTo(TrBottomArea, TrUpPos.localPosition, 0.1f));
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("AnimationView : Pointer is Dragging....");
            Core.Events.EventSystem.DispatchEvent("OnPointerDown3DAniView", (object)true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("AnimationView : Pointer Down.");
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("AnimationView : Pointer Up.");
            Core.Events.EventSystem.DispatchEvent("OnPointerDown3DAniView", (object)false);
        }
        // Transparency View Related.
        public void OnTransparencyViewClicked()
        {
            mTransparencyViewON = !mTransparencyViewON;
            // BtnTransparencyText.text = mTransparencyViewON ? "Transparency OFF" : "Transparency ON"; 

            Core.Events.EventSystem.DispatchEvent("OnTransparencyViewClicked", (object)mTransparencyViewON);
        }
        void EventTransparencyViewTrigger_OnActivated(object data)
        {
            BtnTrasparencyView.GetComponent<Button>().interactable = (bool)data;
            // BtnTransparencyText.text = "Transparency ON";
            mTransparencyViewON = false;
        }
        void EventTransparencyGroupTrigger_OnActivated(object data)
        {
            string key = data == null ? "" : (string)data;
            bool activated = !string.IsNullOrEmpty(key);


            // Testing Default Play Mode.
            mTransparencyViewON = true;//
                                       //false;
            BtnTrasparencyView.GetComponent<Button>().interactable = activated;

            if (mTransparencyViewON)
                Core.Events.EventSystem.DispatchEvent("OnTransparencyViewDefaultPlay", mTransparencyViewON);
        }
        void EventAnimationTrigger_OnActivated(object data)
        {
            BtnTrasparencyView.GetComponent<Button>().interactable = (bool)data;
            // BtnTransparencyText.text = "Transparency ON";
            mTransparencyViewON = false;
        }
        // XRay View Ralated.
        public void OnXRayViewClicked()
        {
            Core.Events.EventSystem.DispatchEvent("OnXRayViewClicked");
        }
        void EventXRayViewTrigger_OnActivated(object data)
        {
            bool activated = (data != null);
            BtnXRayView.GetComponent<Button>().interactable = activated;
        }

        public void EnableLoadingView(bool enable)
        {
            LoadingObject.SetActive(enable);
        }



        //  Private Functions --------------------------------
        //
        IEnumerator CoMoveTo(RectTransform trTarget, Vector3 vTo, float duration, Action whenDone = null)
        {
            Vector3 vStart = trTarget.localPosition;

            float fStartT = Time.time;
            while (Time.time < fStartT + duration)
            {
                trTarget.localPosition = Vector3.Lerp(vStart, vTo, Mathf.Clamp01((Time.time - fStartT) / duration));
                yield return null;
            }
            trTarget.localPosition = vTo;

            if (whenDone != null)
                whenDone.Invoke();
        }
    }

}
