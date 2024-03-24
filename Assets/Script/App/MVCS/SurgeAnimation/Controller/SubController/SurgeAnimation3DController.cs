using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using UnityEngine.Assertions;
using App.Data;
using App.Manager.Data;

namespace App.MVCS
{
    public class SurgeAnimation3DController
    {
        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        //  Properties ------------------------------------
        SurgeAnimationView _view;
        SurgeAnimationModel _model;
        SurgeContext _context;
        SurgeAnimationController _controller;

        bool mIsAppClosing = false;
        bool mIsProgressDragging = false;

        // Camera Control.---------------------------------
        bool mIsPaused = false;
        bool mIsDragging = false;
        Vector3 vOrigin;
        Vector3 vDiffDrag, vDiffRot, vDiffZoom;
        enum eScreenControlMode { eDrag = 0, eZoom, eRotate }
        eScreenControlMode mControlMode = eScreenControlMode.eDrag;
        int mPlaySpeed = 1;

        SurgeAnimation3DView AniView => _view.SurgeAniView;
        public bool Touching3DView { set; private get; } = false;

        XRayData mXRayCache = null;
        bool mIsAnimPrefabLoading = false;
        Coroutine mCoAssetDownloading = null;
        string mLastRecentPathKey = string.Empty;
        string mCurMessageKey = string.Empty;

        public string CurrentPathKey { get; private set; } = string.Empty;
        public int CurrentPathIndexTimeLine { get; private set; } = 0;

        // Public Methodss ---------------------------------
        public SurgeAnimation3DController(SurgeAnimationView view, SurgeAnimationModel model, SurgeAnimationController controller, SurgeContext context)
        {
            _view = view;
            _model = model;
            _context = context;
            _controller = controller;   // main controller.

            Events.RegisterEvent("SurgeAnimation3DView_Update", SurgeAnimation3DView_Update);
            Events.RegisterEvent("SurgeAnimation3DView_LateUpdate", SurgeAnimation3DView_LateUpdate);
            Events.RegisterEvent("SurgeAnimation3DView_OnApplicationQuit", SurgeAnimation3DView_OnApplicationQuit);
            Events.RegisterEvent("SurgeAnimation3DView_OnDisable", SurgeAnimation3DView_OnDisable);

            Events.RegisterEvent("OnProgressBarMSDown", OnProgressBarMSDown);
            Events.RegisterEvent("OnProgressBarMSUp", OnProgressBarMSUp);

            Events.RegisterEvent("OnTimeLineUpdated", OnTimeLineUpdated);

            Events.RegisterEvent("SurgeAnimation3DView_OnScreenControlModeChanged", SurgeAnimation3DView_OnScreenControlModeChanged);
            Events.RegisterEvent("SurgeAnimation3DView_OnPlayClicked", SurgeAnimation3DView_OnPlayClicked);
            Events.RegisterEvent("SurgeAnimation3DView_OnRePlayClicked", SurgeAnimation3DView_OnRePlayClicked);
            Events.RegisterEvent("SurgeAnimation3DView_OnPauseClicked", SurgeAnimation3DView_OnPauseClicked);
            Events.RegisterEvent("SurgeAnimation3DView_OnJumpToNextSector", SurgeAnimation3DView_OnJumpToNextSector);
            Events.RegisterEvent("SurgeAnimation3DView_OnRedoSectorClicked", SurgeAnimation3DView_OnRedoSectorClicked);
            Events.RegisterEvent("SurgeAnimation3DView_OnAnimationPlaySpeedChanged", SurgeAnimation3DView_OnAnimationPlaySpeedChanged);

            Events.RegisterEvent("SurgeAnimation3DView_OnAddNote", SurgeAnimation3DView_OnAddNote);
            Events.RegisterEvent("OnAnimationJumpTo", OnAnimationJumpTo);
            Events.RegisterEvent("OnSectionStarted", OnSectionStarted);
            Events.RegisterEvent("OnProgressBarDragged", OnProgressBarDragged);
            Events.RegisterEvent("OnEventMessageTriggered", OnEventMessageTriggered);

            Events.RegisterEvent("OnPointerDown3DAniView", OnPointerDown3DAniView);

            // Utility View.
            Events.RegisterEvent("EventXRayViewTrigger_OnActivated", EventXRayViewTrigger_OnActivated);
            Events.RegisterEvent("OnXRayViewClicked", OnXRayViewClicked);
            Events.RegisterEvent("EventPathSelectTrigger_OnTrigger", EventPathSelectTrigger_OnTrigger);
        }

        public void Start(GameObject mainPrefab, int indexAnim, float startAniTimeAtBundle = .0f, float delay = .0f, System.Action callbackDone = null)
        {
            Assert.IsTrue(!string.IsNullOrEmpty(_context.AnimationBundleName));

            _controller.AnimBundleIndex = indexAnim;

            float fStartTime = Time.time;
            AniView.StartView(mainPrefab, GetStartingPathKeyForBranchSystem(), () =>
            {
                _view.StartCoroutine(coStartAnimation(Time.time - fStartTime, delay, startAniTimeAtBundle));

                if (callbackDone != null)
                    callbackDone.Invoke();
            });

            mIsProgressDragging = false;
            mControlMode = eScreenControlMode.eDrag;
            mPlaySpeed = 1;
            mXRayCache = null;
            CurrentPathKey = string.Empty;
            CurrentPathIndexTimeLine = 0;
        }








        // Event Handlers ---------------------------------

        void TimeLinePlayAt(double time, double speed)
        {
            mIsPaused = false;
            AniView.TimeLinePlayAt(time, speed);
        }

        void SurgeAnimation3DView_OnDisable(object obj)
        {
            if (mIsAppClosing) return;

            AniView.CleanUp();

            if (mCoAssetDownloading != null)
            {
                _context.ABManager.AbortAllDownloading();
                _view.StopCoroutine(mCoAssetDownloading);

                mCoAssetDownloading = null;
                mIsAnimPrefabLoading = false;
                _view.EnableLoadingView(false);
            }

            mIsPaused = false;
            // _model.SurgeAniInfo = null;
            _context.AnimationBundleName = string.Empty;
        }

        void SurgeAnimation3DView_Update(object obj)
        {
            if (!mIsProgressDragging)
            {
                if (mIsPaused) return;
                if (!AniView.LoadingIsDone) return;
            }
            AniView.UpdateDummyCamera();
        }

        void SurgeAnimation3DView_LateUpdate(object obj)
        {
            if (!mIsPaused) return;
            if (!AniView.LoadingIsDone) return;
            if (mIsProgressDragging) return;

#if UNITY_EDITOR
            UpdateTouchStatus();
            AniView.ReflectDummyTransformToAniCamera();

            if (!Touching3DView) return;
            switch (mControlMode)
            {
                case eScreenControlMode.eDrag:
                    AniView.UpdateDrag(vDiffDrag);
                    break;
                case eScreenControlMode.eZoom:
                    AniView.UpdateZoom(vDiffZoom);
                    break;
                case eScreenControlMode.eRotate:
                    AniView.UpdateRotationAround(vDiffRot);
                    break;
                default:
                    return;
            }
#else
            UpdateTouchStatusDevice();
            AniView.ReflectDummyTransformToAniCamera();

            if (Touching3DView)
            {
                AniView.UpdateDrag(vDiffDrag);
                AniView.UpdateZoom(vDiffZoom);
                AniView.UpdateRotationAround(vDiffRot);
            }
#endif
        }

        void SurgeAnimation3DView_OnApplicationQuit(object obj)
        {
            mIsAppClosing = true;
        }
        void OnProgressBarMSDown(object data)
        {
            mIsProgressDragging = true;
            mIsDragging = false;
        }
        void OnProgressBarMSUp(object data)
        {
            mIsProgressDragging = false;
            mIsDragging = false;
        }
        void SurgeAnimation3DView_OnScreenControlModeChanged(object obj)
        {
            mControlMode = (eScreenControlMode)obj;
        }
        void SurgeAnimation3DView_OnPlayClicked(object obj)
        {
            mIsPaused = false;
            AniView.SetAnimationSpeed(mPlaySpeed);
        }
        void SurgeAnimation3DView_OnRePlayClicked(object obj)
        {
            TimeLinePlayAt(0, mPlaySpeed);
        }
        void SurgeAnimation3DView_OnPauseClicked(object obj)
        {
            AniView.SetAnimationSpeed(0);
            mIsPaused = true;
            vDiffDrag = Vector3.zero;
            vDiffRot = Vector3.zero;
            vDiffZoom = Vector3.zero;
        }
        void SurgeAnimation3DView_OnJumpToNextSector(object obj)
        {
            if (mCoAssetDownloading != null)
                return;

            // find currenct sector.
            var SurgMainInfo = _context.AnimControllerInfoRef;
            long curFrame = _context.Util.TimeToFrame(_controller.GetProgressBarTime((float)AniView.TimeLineCurTime));

            for (int k = 1; k < SurgMainInfo.SectionList.Count; ++k)
            {
                if (curFrame < SurgMainInfo.SectionList[k].Frame)
                {
                    // Check Animation Frame Bandwidth.
                    uint targetFrame = SurgMainInfo.SectionList[k].Frame;
                    CORE_EvaluateTimeLineAt((float)_context.Util.FrameToTime(targetFrame));

                    break;
                }
            }
        }
        void SurgeAnimation3DView_OnRedoSectorClicked(object obj)
        {
            if (mCoAssetDownloading != null)
                return;

            // find currenct sector.
            var SurgMainInfo = _context.AnimControllerInfoRef;
            long curFrame = _context.Util.TimeToFrame(_controller.GetProgressBarTime((float)AniView.TimeLineCurTime));

            for (int k = 1; k < SurgMainInfo.SectionList.Count; ++k)
            {
                if (curFrame < SurgMainInfo.SectionList[k].Frame)
                {
                    // Even close to this frame's start ?
                    if (curFrame <= SurgMainInfo.SectionList[k - 1].Frame + 30 && k - 2 >= 0)
                    {
                        long targetFrame = SurgMainInfo.SectionList[k - 2].Frame;
                        CORE_EvaluateTimeLineAt((float)_context.Util.FrameToTime(targetFrame));
                    }

                    else
                    {
                        long targetFrame = SurgMainInfo.SectionList[k - 1].Frame;
                        CORE_EvaluateTimeLineAt((float)_context.Util.FrameToTime(targetFrame));
                    }
                    break;
                }
            }
        }
        void OnAnimationJumpTo(object data)
        {
            TimeLinePlayAt(ProgressRateToTime((float)data), mPlaySpeed);
        }
        void SurgeAnimation3DView_OnAddNote(object obj)
        {
            var presentData = new NoteDialogView.PresentData();
            presentData.index = -1;
            presentData.MemoContent = string.Empty;
            _context.TriggerDialog("noteDialog", presentData, (returnedData) =>
            {
                var data = returnedData as NoteDialogView.ReturnData;
                EventSystem.DispatchEvent("OnNoteDialogClosed");

                if (!data.ok) return;

                // data update.
                float fRate = TimeToProgressRate(AniView.TimeLineCurTime);
                Debug.Log($"Note Dialog closed...{fRate}, {data.content}");

                NoteData noteData = new NoteData();
                noteData.fTimeRate = fRate;
                noteData.Content = data.content;
                // _context.BootStrap.userData.AddNote(_context.BootStrap.userData.CurrentLearningId, noteData);
                Debug.Log("Note Data has been added...");

                // Refresh View.
                EventSystem.DispatchEvent("OnNoteUpdated", null);
            });

            /*
            GameObject noteDialog = PopupDialogManager.GetInstance().Trigger("notedialog");

            noteDialog.SetActive(true);
            noteDialog.GetComponent<NoteDialog>().Init(-1, string.Empty, (NoteDialog.ReturnData data) =>
            {
            
            });*/
        }

        void OnSectionStarted(object data)
        {
            var SurgMainInfo = _context.AnimControllerInfoRef;
            if (SurgMainInfo == null) return;

            int id = (int)data;
            Debug.Log($"Sector id {id}..");
            // _context.BootStrap.userData.IdCurrentPhase = id;
            // update message.

            int idx = id;// - 1;
            if (idx < 0 || idx >= SurgMainInfo.SectionList.Count)
                return;

            if (string.IsNullOrEmpty(SurgMainInfo.SectionList[idx].Message))
                return;

            AniView.UpdateMessage(SurgMainInfo.SectionList[idx].Message);

            if (idx > 1 && !_context.BootStrap.userData.ExpertMode)
            {
                _view.OnPaused();
                SurgeAnimation3DView_OnPauseClicked(null);
            }
        }

        // Msg Update from Message-Event Trigger.
        void OnEventMessageTriggered(object data)
        {
            var SurgMainInfo = _context.AnimControllerInfoRef;
            if (SurgMainInfo == null) return;

            string key = (string)data;

            // Dumb Search & Update.
            for (int k = 0; k < SurgMainInfo.MessageList.Count; ++k)
            {
                if (key == SurgMainInfo.MessageList[k].Key)
                {
                    mCurMessageKey = key;
                    AniView.UpdateMessage(SurgMainInfo.MessageList[k].Message);
                    break;
                }
            }
        }

        void OnProgressBarDragged(object data)
        {
            //double curTime = ProgressRateToTime((float)data);   // percentage * total-durtaion.

            float percent = (float)data;
            float curTime = percent * _controller.GetTotalProgressBarTime();
            if (curTime <= .0f) return;

            CORE_EvaluateTimeLineAt(curTime);

            // reset control parameters.
            vDiffDrag = Vector3.zero;
            vDiffRot = Vector3.zero;
            vDiffZoom = Vector3.zero;
        }

        void SurgeAnimation3DView_OnAnimationPlaySpeedChanged(object data)
        {
            int index = (int)data;

            switch (index)
            {
                default:
                case 0: mPlaySpeed = 1; break;
                case 1: mPlaySpeed = 2; break;
                case 2: mPlaySpeed = 4; break;
                case 3: mPlaySpeed = 8; break;
            }

            AniView.SetAnimationSpeed(mPlaySpeed);
        }

        void OnPointerDown3DAniView(object data)
        {
            Touching3DView = (bool)data;
        }

        void EventXRayViewTrigger_OnActivated(object data)
        {
            mXRayCache = (XRayData)data;
        }

        void EventPathSelectTrigger_OnTrigger(object data)
        {
            if (data == null)
                return;

            AnimPathInfo targetPathInfo = (AnimPathInfo)data;
            if (targetPathInfo == null) return;

            if (mIsProgressDragging && !targetPathInfo.AllowDialogWhenDragging)
                return;

            if (!string.IsNullOrEmpty(mLastRecentPathKey) && targetPathInfo.Key == mLastRecentPathKey)
                return;

            // Pause Animation Process.
            AniView.SetAnimationSpeed(.0f);     // Jump Func may override this speed.
            AniView.StartCoroutine(coTriggerActionWithDelay(0.2f, () =>
            {
                AniView.SetAnimationSpeed(.0f); // So need to slow the ani down AGAIN.
            }));
            //

            _view.SurgeTopUIView.SetControlButtonGroupVisibility(false);

            // Display dialog to ask anim-path.
            AnimPathSelectionDialogView.PresentData dlgData = new AnimPathSelectionDialogView.PresentData();
            for (int q = 0; q < targetPathInfo.PathOptions.Length; ++q)
            {
                var pathDetailInfo = targetPathInfo.PathOptions[q];
                AnimPathSubView.PathInfo pathOption = new AnimPathSubView.PathInfo();
                pathOption.PathName = pathDetailInfo.DisplayName;
                pathOption.Header = pathDetailInfo.DisplayHeader;

                if (pathDetailInfo.PreviewAnimations != null)
                {
                    for (int k = 0; k < pathDetailInfo.PreviewAnimations.Length; ++k)
                    {
                        AnimPathSubView.PreviewAnimInfo previewAnim = new AnimPathSubView.PreviewAnimInfo();
                        previewAnim.animTarget = pathDetailInfo.PreviewAnimations[k].AnimObject;
                        previewAnim.aniName = pathDetailInfo.PreviewAnimations[k].AniName;

                        if (previewAnim.animTarget != null && !string.IsNullOrEmpty(previewAnim.aniName))
                            pathOption.PreviewAnims.Add(previewAnim);
                    }
                }
                dlgData.PathOptions.Add(pathOption);
            }

            string strOrgMsgKey = mCurMessageKey;
            if(!string.IsNullOrEmpty(targetPathInfo.MessageKey))
                OnEventMessageTriggered((object)targetPathInfo.MessageKey);
            

            _context.TriggerDialog("AnimPathSelectionDialog", dlgData,
                (ret) =>
                {
                    AnimPathSelectionDialogView.ReturnData retData = (AnimPathSelectionDialogView.ReturnData)ret;
                    int idxSelected = retData.IndexSelected;

                    _view.SurgeTopUIView.SetControlButtonGroupVisibility(true);

                    if (idxSelected < 0 || idxSelected >= targetPathInfo.PathOptions.Length)
                    {
                        Assert.IsTrue(false);
                        return;
                    }

                    OnEventMessageTriggered((object)strOrgMsgKey);

                    mLastRecentPathKey = targetPathInfo.Key;
                    // Debug.Log("Last Recent Path Key Updated..." + targetPathInfo.Key);

                    var CurrentPathInfo = targetPathInfo.PathOptions[idxSelected];

                    CurrentPathKey = CurrentPathInfo.PathKey;
                    CurrentPathIndexTimeLine = CurrentPathInfo.TimelineIndex;

                    // Find Path data from the selection.
                    PlayerablePath pathInfo = AniView.PrefabMain.GetPlayerablePath(CurrentPathInfo.PathKey);
                    if (pathInfo == null) return;

                    int idxTimeline = CurrentPathInfo.TimelineIndex;
                    if (idxTimeline < 0 || idxTimeline >= pathInfo.TimeLine.Length)
                    {
                        Assert.IsTrue(false, $"Invalid Timeline index : {idxTimeline} !!!");
                        return;
                    }

                    // Update Control config.
                    for (int z = 0; z < _context.PathKeyControllerListInfoRef.Count; ++z)
                    {
                        if (_context.PathKeyControllerListInfoRef[z].Item1 == pathInfo.Key)
                        {
                            _context.AnimControllerInfoRef = _context.PathKeyControllerListInfoRef[z].Item2;
                            // Debug.Log("Path Ctrl Config has been udpated. " + pathInfo.Key);
                            break;
                        }
                    }

                    // Update UI progress bar.
                    EventSystem.DispatchEvent("OnAnimControllerUpdatedBySelectingPath");


                    // Update Playable Asset.
                    AniView.SetAnimationSpeed(1.0f);
                    AniView.SetPlayableAsset(pathInfo.TimeLine[idxTimeline]);

                    // Play at cut-in time.
                    AniView.TimeLinePlayAt(_context.Util.FrameToTime(CurrentPathInfo.StartFrame), speed: 1);

                    const float PathDialogCoolTime = 1.0f;
                    AniView.StartCoroutine(coTriggerActionWithDelay(PathDialogCoolTime, () =>
                    {
                        mLastRecentPathKey = string.Empty;
                        Debug.Log("Last Recent Path Key Cleared...!");
                    }));

                });
        }

        void OnXRayViewClicked(object data)
        {
            if (mXRayCache == null)
                return;

            var presentData = new XRayDialogView.PresentData();
            presentData.Context = _context;
            for (int k = 0; k < mXRayCache.ShotInfos.Length; ++k)
            {
                presentData.SpriteBtns.Add(mXRayCache.ShotInfos[k].Shot);
                presentData.SpritesZoomIn.Add(mXRayCache.ShotInfos[k].Shot);
                presentData.PicNames.Add(mXRayCache.ShotInfos[k].Name);
            }

            _context.TriggerDialog("xrayDialog", presentData, (returnedData) =>
            {
                var data = returnedData as XRayDialogView.ReturnData;
                // if (!data.ok) return;

                // Refresh View.
                EventSystem.DispatchEvent("OnXRayDialogClosed");
            });
        }

        void OnTimeLineUpdated(object data)
        {
            var curTime = (double)data;
            long aniStartFrm = _context.AnimSurgeInfoRef.BundleDependencies[_controller.AnimBundleIndex].StartFrame;
            long curFrame = aniStartFrm + _context.Util.TimeToFrame(curTime);

            // Check Animation Frame Bandwidth.
            int idxAnim = GetAnimIndexByFrame(curFrame);
            if (idxAnim >= 0 && idxAnim != _controller.AnimBundleIndex)
            {
                aniStartFrm = _context.AnimSurgeInfoRef.BundleDependencies[idxAnim].StartFrame;
                double startT = _context.Util.FrameToTime(curFrame - aniStartFrm);
                mCoAssetDownloading = _view.StartCoroutine(coLoadPrefab(idxAnim, (float)startT, () => { }));
            }
            else
            {


                PlayerablePath pathInfo = _view.SurgeAniView.PrefabMain.GetPlayerablePath(CurrentPathKey);
                if (pathInfo != null)
                {
                    if (CurrentPathIndexTimeLine >= 0 && CurrentPathIndexTimeLine < pathInfo.TimeLine.Length - 1)
                    {
                        if (curTime >= pathInfo.TimeLine[CurrentPathIndexTimeLine].duration)
                        {
                            Debug.Log("TimeLine has been jumped! ");
                            ++CurrentPathIndexTimeLine;
                            _view.SurgeAniView.SetPlayableAsset(pathInfo.TimeLine[CurrentPathIndexTimeLine]);
                            TimeLinePlayAt(.0f, mPlaySpeed);
                        }
                    }
                }

            }
        }


        IEnumerator coLoadPrefab(int idxAnim, float startAniTimeAtBundle, System.Action callbackDone = null)
        {
            if (mIsAnimPrefabLoading)
                yield break;

            mIsAnimPrefabLoading = true;
            _view.EnableLoadingView(true);

            //var controlData = _context.AnimCtrlFetcher.ControlInfo;

            string prefabName = _context.AnimSurgeInfoRef.BundleDependencies[idxAnim].Prefab;
            prefabName = _context.BootStrap.setting.UseRemoteBundle ? prefabName : prefabName + ".prefab";

            AssetBundle animBundle = null;
            string bundleName = _context.AnimSurgeInfoRef.BundleDependencies[idxAnim].Name;
            float speed = AniView.GetAnimationSpeed();
            if (_context.BootStrap.setting.UseRemoteBundle)
            {
                // Wait Until the Bundle is (down)loaded. 
                while (true)
                {
                    if (_context.ABManager.IsCached(bundleName))
                        break;
                    else
                    {
                        // Pause anim play until load.
                        AniView.SetAnimationSpeed(0);
                        yield return new WaitForSeconds(0.2f);
                    }
                }

                // Fetch Bundle.
                _view.StartCoroutine(_context.ABManager.FetchBundle(bundleName, (loaded) => animBundle = loaded, null));
                yield return new WaitUntil(() => animBundle != null);
            }

            yield return _view.StartCoroutine(_context.CoLoadAssetFromBundle(animBundle, prefabName, _context.AnimSurgeInfoRef.BundleDependencies[idxAnim].Name));
            Assert.IsTrue(_context.AnimationBundlePrefab != null);
            if (speed > .0f)
                AniView.SetAnimationSpeed(speed);

            AniView.CleanUp();
            Start(_context.AnimationBundlePrefab, idxAnim, startAniTimeAtBundle, delay: 0.0f, () =>
             {
                 mIsAnimPrefabLoading = false;
                 _view.EnableLoadingView(false);

                 if (callbackDone != null)
                     callbackDone.Invoke();
             });
            mCoAssetDownloading = null;
        }

        int GetAnimIndexByFrame(long frame)
        {
            for (int q = 0; q < _context.AnimSurgeInfoRef.BundleDependencies.Count - 1; ++q)
            {
                if (frame >= _context.AnimSurgeInfoRef.BundleDependencies[q].StartFrame &&
                    frame < _context.AnimSurgeInfoRef.BundleDependencies[q + 1].StartFrame)
                    return q;
            }
            return _context.AnimSurgeInfoRef.BundleDependencies.Count - 1;
        }






        // Private Functions ------------------------------
        //
        void UpdateTouchStatus()
        {
#if !UNITY_EDITOR
            // Editor Only Func. 
            return;
#endif
            if (Input.GetMouseButton(0))
            {
                if (mIsDragging == false)
                {
                    mIsDragging = true;
                    vOrigin = Input.mousePosition;
                }
                vDiffDrag = Input.mousePosition - vOrigin;
                vDiffRot = Input.mousePosition - vOrigin;
                vDiffZoom = Input.mousePosition - vOrigin;
            }
            else
            {
                mIsDragging = false;
                vDiffDrag = Vector3.zero;
                vDiffRot = Vector3.zero;
                vDiffZoom = Vector3.zero;
            }

            if (mIsDragging == true)
                vOrigin = Input.mousePosition;
        }

        string GetStartingPathKeyForBranchSystem()
        {
            if (_context.PathKeyControllerListInfoRef == null || _context.PathKeyControllerListInfoRef.Count <= 0)
                return string.Empty;

            if (_context.AnimBranchControllerInfoRef.BranchSystem == null)
                return string.Empty;

            int idxStart = _context.AnimBranchControllerInfoRef.BranchSystem.StartingPathIndex;
            if (idxStart >= 0 && idxStart < _context.PathKeyControllerListInfoRef.Count)
                return _context.PathKeyControllerListInfoRef[idxStart].Item1;

            return string.Empty;
        }

        void CORE_EvaluateTimeLineAt(float timeOnProgressBar)
        {
            Assert.IsTrue(_context.AnimSurgeInfoRef != null);
            Assert.IsTrue(_context.AnimSurgeInfoRef.BundleDependencies != null);
            Assert.IsTrue(_context.AnimSurgeInfoRef.BundleDependencies.Count >= 1);

            // ProgressBar Time.
            long frameOnProgressBar = _context.Util.TimeToFrame(timeOnProgressBar);
            int idxAnimBundle = GetAnimIndexByFrame(frameOnProgressBar);
            bool validAnimBundleIdx = idxAnimBundle >= 0 && idxAnimBundle < _context.AnimSurgeInfoRef.BundleDependencies.Count;

            // => Bundle Anim Time.
            long bundleStartFrame = validAnimBundleIdx ? _context.AnimSurgeInfoRef.BundleDependencies[idxAnimBundle].StartFrame : 0;
            float timeAtBundle = (float)_context.Util.FrameToTime(frameOnProgressBar - bundleStartFrame);
            if (idxAnimBundle >= 0 && idxAnimBundle != _controller.AnimBundleIndex)
            {
                // Reload Anim Bundle if necessary.
                mCoAssetDownloading = _view.StartCoroutine(coLoadPrefab(idxAnimBundle, timeAtBundle, () => { }));
                return;
            }

            // => TimeLine Time.
            float timeAtTimeLine = SetPathTimeLineByCurrentTime(timeAtBundle);

            // Finally, Play the TimeLine at point.--- AniView.EvaluateTimeline( timeAtTimeLine );
            TimeLinePlayAt(timeAtTimeLine, mPlaySpeed);
        }

        float SetPathTimeLineByCurrentTime(float curTime)
        {
            PlayerablePath pathInfo = _view.SurgeAniView.PrefabMain.GetPlayerablePath(CurrentPathKey);
            if (pathInfo != null)
            {
                for (int k = 0; k < pathInfo.TimeLine.Length; ++k)
                {
                    if (curTime > pathInfo.TimeLine[k].duration)
                        curTime -= (float)pathInfo.TimeLine[k].duration;

                    else
                    {
                        CurrentPathIndexTimeLine = k;
                        _view.SurgeAniView.SetPlayableAsset(pathInfo.TimeLine[k]);
                        break;
                    }
                }
            }
            return curTime;
        }

        void UpdateTouchStatusDevice()
        {
            // Device Only Func.
#if !UNITY_EDITOR
        
            if (Input.GetMouseButton(0) && Input.touchCount==1)
            {
                if (mIsDragging == false)
                {
                    mIsDragging = true;
                    vOrigin = Input.mousePosition;
                }
                vDiffDrag = Input.mousePosition - vOrigin;
            }
            else
            {
                mIsDragging = false;
                vDiffDrag = Vector3.zero;
            }

            if (mIsDragging == true)
                vOrigin = Input.mousePosition;


            // checkin multi touch.
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Calculating zoom pinch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                vDiffZoom = new Vector3(.0f, currentMagnitude - prevMagnitude, .0f);

                // Rotation.
                vDiffRot = touchZero.deltaPosition;
            }
            else
            {
                vDiffZoom = Vector3.zero;
                vDiffRot = Vector3.zero;
            }
#endif
        }

        IEnumerator coStartAnimation(float fLoadingTime, float fDelay, float fStartTimeAtBundle)
        {
            // Consume StartingDelayInSecond.
            if (fDelay > .0f)
            {
                float fStartDelay = fDelay > fLoadingTime ? fDelay - fLoadingTime : .0f;
                Debug.Log($"Animation Starting Delay..{fDelay}, {fStartDelay}");
                yield return new WaitForSeconds(fStartDelay);
            }

            float fTimeAtTimeLine = SetPathTimeLineByCurrentTime(fStartTimeAtBundle);
            TimeLinePlayAt(fTimeAtTimeLine, mPlaySpeed);
        }

        IEnumerator coTriggerActionWithDelay(float delay, System.Action callback)
        {
            yield return new WaitForSeconds(delay);

            callback.Invoke();
        }

        // percent should be in range (0.0 ~ 1.0f)
        double ProgressRateToTime(float percent)
        {
            var SurgMainInfo = _context.AnimControllerInfoRef;
            if (SurgMainInfo == null) return 0;

            List<SectionInfo> sections = SurgMainInfo.SectionList;
            double aniDuration = _context.Util.FrameToTime((long)sections[sections.Count - 1].Frame);
            //(float)AniView.TimeLineDuration;
            return percent * (float)aniDuration;
        }
        float TimeToProgressRate(double time)
        {
            var SurgMainInfo = _context.AnimControllerInfoRef;
            if (SurgMainInfo == null)
                return .0f;

            List<SectionInfo> sections = SurgMainInfo.SectionList;
            double aniDuration = _context.Util.FrameToTime((long)sections[sections.Count - 1].Frame);
            float curTime = (float)AniView.TimeLineCurTime;

            return (float)(curTime / aniDuration);
        }
    }

}