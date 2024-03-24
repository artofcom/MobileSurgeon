using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using Core.Events;
using System;
using App.Common;

namespace App.MVCS
{
    public class SurgeAnimation3DView : MonoBehaviour
    {
        // [SerializeField] ------------------------------
        [SerializeField] TMP_Text txtDescription;
        [SerializeField] float ScreenDragRate = 0.001f;
        [SerializeField] float ScreenZoomRate = 0.001f;
        [SerializeField] float ScreenRotateRate = 0.05f;
        [SerializeField] float ScreenRotateAroundDist = 10.0f;
        [SerializeField] Transform DummyCamSimulation;  // need dummy transform, since tr for camera should be overriden by anim.


        // Debug Controller. -----------------------------
        [SerializeField] GameObject BtnPlay, BtnPause;
        [SerializeField] TMP_Dropdown ScreenControlMode;
        [SerializeField] TMP_Dropdown DDAnimationSpeed;


        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();



        // Properties ------------------------------------
        //
        PlayableDirector TimeLineDirector = null;
        private Coroutine mCoUpdator = null;
        GameObject mMainPrefabCached;
        bool mRequiredResumeWhenDone = false;


        // Camera Control.
        List<Transform> AniCameras = new List<Transform>();
        bool mIsUpdating = false;
        Transform RotationTarget;
        bool mUseRevertedCamera = false;
        bool mTimeLineFinished = false;
        float mLastPlayerableDuration = .0f;

        // Public Methodss ---------------------------------
        #region PUBLIC METHODS
        public bool LoadingIsDone => (TimeLineDirector != null);

        public double TimeLineDuration => (TimeLineDirector == null || TimeLineDirector.playableAsset == null) ? (mTimeLineFinished ? mLastPlayerableDuration : 0) : TimeLineDirector.playableAsset.duration;

        public double TimeLineCurTime => (TimeLineDirector == null || TimeLineDirector.playableAsset == null) ? (mTimeLineFinished ? mLastPlayerableDuration : 0) : (mTimeLineFinished ? mLastPlayerableDuration : TimeLineDirector.time);

        public AniPrefabMain PrefabMain { get; private set; } = null;

        public void StartView(GameObject mainPrefab, string startPathPlayableAssetKey, Action callback)
        {
            mMainPrefabCached = mainPrefab;
            mRequiredResumeWhenDone = false;

#if !UNITY_EDITOR
            ScreenControlMode.gameObject.SetActive(false);
            DDAnimationSpeed.gameObject.SetActive(false);
#endif
            // StartCoroutine(coLoadSurgAni(callback));
            // StartCoroutine(coLoadSurgAniFromAssetBundle2(callback));

            Events.RegisterEvent("OnTransparencyViewClicked", OnTransparencyViewClicked); Events.RegisterEvent("OnXRayViewClicked", OnXRayViewClicked);
            Events.RegisterEvent("OnXRayDialogClosed", OnXRayDialogClosed);
            Events.RegisterEvent("OnNoteDialogClosed", OnNoteDialogClosed);
            Events.RegisterEvent("OnNoteItemClicked", OnNoteItemClicked);


            StartCoroutine(coLoadSurgAniFromAssetBundle(startPathPlayableAssetKey, callback));
        }
        public void CleanUp()
        {
            GameObject.Destroy(TimeLineDirector.gameObject);

            if (mCoUpdator != null)
                StopCoroutine(mCoUpdator);

            AniCameras.Clear();
            mCoUpdator = null;
            mIsUpdating = false;
            TimeLineDirector = null;
            mTimeLineFinished = false;
        }
        public void UpdateDummyCamera()
        {
            if (AniCameras.Count <= 0) return;

            DummyCamSimulation.position = AniCameras[0].transform.position;
            DummyCamSimulation.localRotation = AniCameras[0].transform.localRotation;
        }
        public void SetAnimationSpeed(float speed)
        {
            if (TimeLineDirector != null)
                TimeLineDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }
        public float GetAnimationSpeed()
        {
            if (TimeLineDirector != null)
                return (float)TimeLineDirector.playableGraph.GetRootPlayable(0).GetSpeed();

            return -1.0f;
        }
        public void UpdateMessage(string message)
        {
            if (txtDescription != null)
                txtDescription.text = message;
        }
        public void EvaluateTimeline(double time, bool playWhenReady = true)
        {
            if (TimeLineDirector == null) return;

            if (playWhenReady && TimeLineDirector.state != PlayState.Playing)
                TimeLineDirector.Play();

            TimeLineDirector.time = time;
            TimeLineDirector.Evaluate();
            mTimeLineFinished = false;
        }
        public void TimeLinePlayAt(double time, double speed)
        {
            TimeLineDirector.time = time;
            TimeLineDirector.Evaluate();
            TimeLineDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
            TimeLineDirector.Play();

            BtnPlay.SetActive(false);
            BtnPause.SetActive(true);
            mTimeLineFinished = false;

            if (mCoUpdator == null)
            {
                mCoUpdator = StartCoroutine(coUpdateTimeLine());
            }
        }
        public void ReflectDummyTransformToAniCamera()
        {
            // aniCamera data is dirty since its animation transform data.
            for (int k = 0; k < AniCameras.Count; ++k)
            {
                AniCameras[k].transform.position = DummyCamSimulation.position;
                AniCameras[k].transform.rotation = DummyCamSimulation.rotation;
            }
        }

        public void UpdateZoom(Vector3 vDifference)
        {
            float fRevert = mUseRevertedCamera ? -1.0f : 1.0f;
            DummyCamSimulation.position += DummyCamSimulation.forward * fRevert * vDifference.y * ScreenZoomRate;
            for (int k = 0; k < AniCameras.Count; ++k)
                AniCameras[k].transform.position += DummyCamSimulation.forward * fRevert * vDifference.y * ScreenZoomRate;
        }

        public void UpdateDrag(Vector3 vDifference)
        {
            float fRevert = mUseRevertedCamera ? -1.0f : 1.0f;
            DummyCamSimulation.position -= DummyCamSimulation.right * fRevert * vDifference.x * ScreenDragRate;
            DummyCamSimulation.position -= DummyCamSimulation.up * vDifference.y * ScreenDragRate;
            for (int k = 0; k < AniCameras.Count; ++k)
            {
                AniCameras[k].transform.position -= DummyCamSimulation.right * fRevert * vDifference.x * ScreenDragRate;
                AniCameras[k].transform.position -= DummyCamSimulation.up * vDifference.y * ScreenDragRate;
            }
        }

        public void UpdateRotationAround(Vector3 vDifference)
        {
            float fRevert = mUseRevertedCamera ? -1.0f : 1.0f;
            Vector3 vTarget = RotationTarget != null ? RotationTarget.position : DummyCamSimulation.position + DummyCamSimulation.forward * ScreenRotateAroundDist;
            Debug.DrawLine(DummyCamSimulation.position, vTarget);

            for (int k = 0; k < AniCameras.Count; ++k)
            {
                AniCameras[k].transform.RotateAround(vTarget, Vector3.up, vDifference.x * ScreenRotateRate);
                AniCameras[k].transform.RotateAround(vTarget, AniCameras[k].transform.right, fRevert * -vDifference.y * ScreenRotateRate);

            }
            DummyCamSimulation.RotateAround(vTarget, Vector3.up, vDifference.x * ScreenRotateRate);
            DummyCamSimulation.RotateAround(vTarget, AniCameras[0].transform.right, fRevert * -vDifference.y * ScreenRotateRate);
        }

        public void OnScreenControlModeChanged(int index)
        {
            if (!LoadingIsDone) return;

            EventSystem.DispatchEvent("SurgeAnimation3DView_OnScreenControlModeChanged", (object)index);

            Debug.Log($"Screen Mode changed! {index}");
            //mControlMode = (eScreenControlMode)index;
        }
        public void OnAnimationPlaySpeedChanged(int index)
        {
            if (!LoadingIsDone) return;

            EventSystem.DispatchEvent("SurgeAnimation3DView_OnAnimationPlaySpeedChanged", (object)index);
        }
        public void SetPlayableAsset(PlayableAsset newAsset)
        {
            if (TimeLineDirector != null)
                TimeLineDirector.playableAsset = newAsset;
        }

        #endregion



        // Mono Callbacks ---------------------------------
        #region Unity CallBacks
        void Start()
        {
            //RenderPipelineManager.beginCameraRendering += OnBeginCameraRender;
            //RenderPipelineManager.endCameraRendering += OnPostCameraRender;
        }
        private void OnDisable()
        {
            EventSystem.DispatchEvent("SurgeAnimation3DView_OnDisable");
        }
        void OnApplicationQuit()
        {
            EventSystem.DispatchEvent("SurgeAnimation3DView_OnApplicationQuit");
        }
        private void Update()
        {
            if (!LoadingIsDone) return;
            EventSystem.DispatchEvent("SurgeAnimation3DView_Update");
        }
        private void LateUpdate()
        {
            if (!LoadingIsDone) return;
            EventSystem.DispatchEvent("SurgeAnimation3DView_LateUpdate");
        }
        private void OnDestroy()
        {
            //RenderPipelineManager.beginCameraRendering -= OnBeginCameraRender;
            //RenderPipelineManager.endCameraRendering -= OnPostCameraRender;
        }
        #endregion




        // Event Callbacks ---------------------------------
        #region Button & Event Receivers
        public void OnPlayClicked()
        {
            if (!LoadingIsDone) return;
            EventSystem.DispatchEvent("SurgeAnimation3DView_OnPlayClicked");

            BtnPlay.SetActive(false);
            BtnPause.SetActive(true);
        }
        public void OnRePlayClicked()
        {
            if (!LoadingIsDone) return;
            EventSystem.DispatchEvent("SurgeAnimation3DView_OnRePlayClicked");
        }
        public void OnPauseClicked()
        {
            if (!LoadingIsDone) return;

            EventSystem.DispatchEvent("SurgeAnimation3DView_OnPauseClicked");
            BtnPlay.SetActive(true);
            BtnPause.SetActive(false);
        }
        public void OnJumpToNextSector()
        {
            if (!LoadingIsDone) return;

            EventSystem.DispatchEvent("SurgeAnimation3DView_OnJumpToNextSector");
        }
        public void OnRedoSectorClicked()
        {
            if (!LoadingIsDone) return;

            EventSystem.DispatchEvent("SurgeAnimation3DView_OnRedoSectorClicked");
        }
        public void OnAddNote()
        {
            if (!LoadingIsDone) return;

            OnPauseClicked();
            EventSystem.DispatchEvent("SurgeAnimation3DView_OnAddNote");
        }
        void OnNoteItemClicked(object data)
        {
            OnPauseClicked();
        }
        void OnNoteDialogClosed(object data)
        {
            OnPlayClicked();
        }
        void OnTransparencyViewClicked(object data)
        {
            bool enabled = (bool)data;

            if (enabled)
            {
                if (BtnPause.activeSelf)        // Able to pause ?
                {
                    mRequiredResumeWhenDone = true;
                    OnPauseClicked();
                }
            }
            else
            {
                if (BtnPlay.activeSelf &&       // Able to play ?
                    mRequiredResumeWhenDone)    // Need to resume ?
                {
                    OnPlayClicked();
                }
                mRequiredResumeWhenDone = false;
            }
        }
        void OnXRayViewClicked(object data)
        {
            OnPauseClicked();
        }
        void OnXRayDialogClosed(object data)
        {
            OnPlayClicked();
        }
        #endregion


        // Private Methods ---------------------------------
        //
        #region Helper functions

        IEnumerator coLoadSurgAniFromAssetBundle(string startPathPlayableAssetKey, Action callback)
        {
            if (mMainPrefabCached == null)
            {
                UnityEngine.Assertions.Assert.IsTrue(false, "MainPrefab should not be null!");
                yield break;
            }

            // Same Flow as coLoadSurgAni from below.
            GameObject surgPref = mMainPrefabCached;
            GameObject prefabMainObj = GameObject.Instantiate(surgPref, transform);
            UnityEngine.Assertions.Assert.IsTrue(prefabMainObj != null);
            TimeLineDirector = prefabMainObj.GetComponent<PlayableDirector>();
            TimeLineDirector.stopped += (playableDirector) =>
            {
                mTimeLineFinished = true;
                Debug.Log("Timeline has been finished!");
            };

            PrefabMain = prefabMainObj.GetComponent<AniPrefabMain>();
            if (PrefabMain == null)
            {
                UnityEngine.Assertions.Assert.IsTrue(false, "AniPrefabMain Component is missing!");
                yield break;
            }

            // Reset Playable Asset ONLY if We have Multi Paths.
            /* if (!string.IsNullOrEmpty(startPathPlayableAssetKey))
             {
                 PlayableAsset defaultPathPlayableAsset = PrefabMain.GetPathPlayableAsset(startPathPlayableAssetKey);
                 if (defaultPathPlayableAsset != null)
                 {
                     TimeLineDirector.playableAsset = defaultPathPlayableAsset;
                     yield return null;
                 }
             }*/

            mLastPlayerableDuration = (float)TimeLineDirector.playableAsset.duration;
            mTimeLineFinished = false;
            yield return null;

            var rt = TimeLineDirector.transform.GetComponentInChildren<RotateTarget>();
            RotationTarget = rt.transform;

            this.EvaluateTimeline(.0f, playWhenReady: false);
            // TimeLineDirector.Stop();
            yield return null;

            ScreenControlMode.value = 0;
            DDAnimationSpeed.value = 0;

            EventSystem.DispatchEvent("OnAnimaionMainPrefabLoaded", (object)prefabMainObj);

            AniCameras.Clear();
            mUseRevertedCamera = false;

            // Cam setting.
            mUseRevertedCamera = PrefabMain.IsUseRevertedCamera;
            if (PrefabMain.CameraAnimationTransforms != null)
            {
                for (int k = 0; k < PrefabMain.CameraAnimationTransforms.Length; ++k)
                    AniCameras.Add(PrefabMain.CameraAnimationTransforms[k]);
            }

            UnityEngine.Assertions.Assert.IsTrue(AniCameras.Count > 0, "We need at least one camera here!");

            if (callback != null)
                callback.Invoke();
        }

        IEnumerator coUpdateTimeLine()
        {
            if (mIsUpdating) yield break;

            var waitForSeconds = new WaitForSeconds(0.05f);
            mIsUpdating = true;
            while (true)
            {
                yield return waitForSeconds;
                if (TimeLineDirector != null)
                {
                    if (TimeLineDirector.time > 0)
                        EventSystem.DispatchEvent("OnTimeLineUpdated", (object)TimeLineDirector.time);
                }
            }
        }
        #endregion
    }

}