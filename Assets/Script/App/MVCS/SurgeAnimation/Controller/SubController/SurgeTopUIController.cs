using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Core.Events;

namespace App.MVCS
{
    // Sub Controller.
    public class SurgeTopUIController
    {
        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        //  Properties ------------------------------------
        SurgeAnimationView _view;
        SurgeAnimationModel _model;
        SurgeContext _context;
        SurgeAnimationController _controller;
        SurgeAnimationService _service;

        SurgeTopUIView TopUIView => _view.SurgeTopUIView;
        float mCurPreviewProgress;


        // Public Methodss ---------------------------------
        public SurgeTopUIController(SurgeAnimationView view, SurgeAnimationModel model, SurgeAnimationService service, SurgeAnimationController controller, SurgeContext context)
        {
            _view = view;
            _model = model;
            _context = context;
            _service = service;
            _controller = controller;   // main controller.

            Events.RegisterEvent("OnTimeLineUpdated", OnTimeLineUpdated);
            Events.RegisterEvent("OnNoteUpdated", OnNoteUpdated);
            Events.RegisterEvent("OnNoteItemClicked", OnNoteItemClicked);
            Events.RegisterEvent("OnProgressBarClicked", OnProgressBarClicked);
            Events.RegisterEvent("SurgeTopUIView_OnThumbnailPreviewClicked", SurgeTopUIView_OnThumbnailPreviewClicked);

            Events.RegisterEvent("EventXRayViewTrigger_OnMessageActivated", EventXRayViewTrigger_OnMessageActivated);
            Events.RegisterEvent("EventTransparencyViewTrigger_OnActivated", EventTransparencyViewTrigger_OnActivated);
            Events.RegisterEvent("EventTransparencyGroupTrigger_OnActivated", EventTransparencyGroupTrigger_OnActivated);

        }
        public void Start(int AnimIndex)
        {
            var controlData = _context.AnimControllerInfoRef;
            SurgeTopUIView.PresentData topUIData = new SurgeTopUIView.PresentData();
            topUIData.CurTime = (float)_context.Util.FrameToTime(_context.AnimSurgeInfoRef.BundleDependencies[AnimIndex].StartFrame);
            topUIData.TotalTime = (float)_context.Util.FrameToTime(controlData.SectionList[controlData.SectionList.Count - 1].Frame);
            topUIData.CurSector = 1;
            topUIData.TotalSector = controlData.SectionList.Count;
            topUIData.IsExpertMode = _context.BootStrap.userData.ExpertMode;
            topUIData.Title = controlData.CPTCode;

            SurgeTopUIView.PresentData4Note noteData = new SurgeTopUIView.PresentData4Note();
            //string curLearningId = _context.BootStrap.userData.CurrentLearningId;
            //if (!string.IsNullOrEmpty(curLearningId) && _context.BootStrap.userData.DictNotes.ContainsKey(curLearningId))
            {
                //List<NoteData> listData = _context.BootStrap.userData.DictNotes[_context.BootStrap.userData.CurrentLearningId];
                //noteData.Timings = new List<float>();
                //for (int k = 0; k < listData.Count; ++k)
                //    noteData.Timings.Add(listData[k].fTimeRate);
            }

            uint fullFrm = controlData.SectionList[controlData.SectionList.Count - 1].Frame;
            for (int q = 0; q < controlData.SectionList.Count - 1; ++q)
            {
                topUIData.SectionSplitter.Add((float)(controlData.SectionList[q].Frame) / (float)fullFrm);
            }
            for (int q = 0; q < controlData.GroupList.Count; ++q)
            {
                topUIData.GroupSplitter.Add((float)(controlData.GroupList[q].Frame) / (float)(fullFrm));
                topUIData.GroupNames.Add(controlData.GroupList[q].Name);
            }
            // topUIData.TotalFrameCount = fullFrm;

            //_context.BootStrap.userData.IdCurrentPhase = topUIData.CurSector;


            // Assets to download check.
            if (_context.BootStrap.setting.UseRemoteBundle)
            {
                List<string> downloadBundles = new List<string>();
                for (int k = 1; k < _context.AnimSurgeInfoRef.BundleDependencies.Count; ++k)
                {
                    if (!_context.ABManager.IsCached(_context.AnimSurgeInfoRef.BundleDependencies[k].Name))
                        downloadBundles.Add(_context.AnimSurgeInfoRef.BundleDependencies[k].Name);
                }
                if (downloadBundles.Count > 0)
                {
                    int downloaded = 0;
                    _service.DownloadBundles(_view, downloadBundles,
                        (downloadedBundle) =>
                        {
                            ++downloaded;
                            if (downloaded == downloadBundles.Count)
                                EventSystem.DispatchEvent("OnBackgroundAssetDownloadingFinished");
                        },
                        (progress) =>
                        {
                            string strMessage = $"Downloading Assets ({downloaded + 1}/{downloadBundles.Count}) - {(int)(100.0f * progress)}%";
                            EventSystem.DispatchEvent("OnBackgroundAssetDownloadingProgress", (object)strMessage);
                        });
                }
            }

            TopUIView.StartView(topUIData, noteData);
        }






        //  Event Handlers --------------------------------
        private void OnTimeLineUpdated(object data)
        {
            var controlData = _context.AnimControllerInfoRef;
            if (controlData == null)
                return;

            var curTime = (double)data;
            // Debug.Log($"CurTime : {curTime}");
            SurgeTopUIView.PresentData topUIData = new SurgeTopUIView.PresentData();
            topUIData.CurTime = _controller.GetProgressBarTime((float)curTime);
            topUIData.TotalTime = _controller.GetTotalProgressBarTime();

            //topUIData.CurSector = _context.BootStrap.userData.IdCurrentPhase;
            topUIData.TotalSector = controlData.SectionList.Count;
            topUIData.IsExpertMode = _context.BootStrap.userData.ExpertMode;
            topUIData.Title = controlData.CPTCode;
            TopUIView.RefreshUX(topUIData);
        }
        private void OnNoteUpdated(object data)
        {
            /*SurgeTopUIView.PresentData4Note noteData = new SurgeTopUIView.PresentData4Note();
            if (_context.BootStrap.userData.DictNotes.ContainsKey(_context.BootStrap.userData.CurrentLearningId))
            {
                List<NoteData> listData = _context.BootStrap.userData.DictNotes[_context.BootStrap.userData.CurrentLearningId];
                noteData.Timings = new List<float>();
                for (int k = 0; k < listData.Count; ++k)
                    noteData.Timings.Add(listData[k].fTimeRate);
            }
            TopUIView.RefreshNoteUX(noteData);*/
        }
        void OnNoteItemClicked(object data)
        {
            float timeRate = (float)data;
            /*
            var presentData = new NoteDialogView.PresentData();
            int idx = _context.BootStrap.userData.GetNoteIndex(_context.BootStrap.userData.CurrentLearningId, timeRate);
            var noteInfo = _context.BootStrap.userData.GetNoteByIndex(_context.BootStrap.userData.CurrentLearningId, idx);
            presentData.index = idx;
            presentData.MemoContent = noteInfo==null ? string.Empty : noteInfo.Content;

            _context.TriggerDialog("noteDialog", presentData, (returnedData) =>
            {
                var data = returnedData as NoteDialogView.ReturnData;
                EventSystem.DispatchEvent("OnNoteDialogClosed");

                if (!data.ok) return;

                // data update.
                //float fRate = (float)(TimeLineDirector.time / TimeLineDirector.playableAsset.duration);
                //Debug.Log($"Note Dialog closed...{fRate}, {data.content}");

                if (data.jump)
                {
                    EventSystem.DispatchEvent("OnAnimationJumpTo", (object)timeRate);
                    return;
                }

                if (data.delete)
                {
                    _context.BootStrap.userData.RemoveNote(_context.BootStrap.userData.CurrentLearningId, timeRate);
                    Debug.Log("Note Data has been Deleted...");
                }
                else
                {
                    NoteData noteData = new NoteData();
                    noteData.fTimeRate = timeRate;
                    noteData.Content = data.content;
                    _context.BootStrap.userData.UpdateNote(_context.BootStrap.userData.CurrentLearningId, noteData);
                    Debug.Log("Note Data has been Updated...");
                }

                // Refresh View.
                EventSystem.DispatchEvent("OnNoteUpdated", null);
            });*/
        }

        void OnProgressBarClicked(object data)
        {
            Debug.Log("Clicked..... " + (float)data);
        }
        void SurgeTopUIView_OnThumbnailPreviewClicked(object data)
        {
            EventSystem.DispatchEvent("OnAnimationJumpTo", (object)mCurPreviewProgress);
        }

        // data => string. 
        void EventXRayViewTrigger_OnMessageActivated(object data)
        {
            bool activated = (data != null);
            string strKey = activated ? (string)data : "";

            var controlData = _context.AnimControllerInfoRef;

            // dumb search again.
            if (!string.IsNullOrEmpty(strKey))
            {
                for (int k = 0; k < controlData.XRayList.Count; ++k)
                {
                    if (strKey == controlData.XRayList[k].Key)
                    {
                        _view.TriggerXRayMessage(controlData.XRayList[k].Message);
                        return;
                    }
                }

                // Valid Key but couldn't find data.
                string defaultMessage = "XRay";
                _view.TriggerXRayMessage(defaultMessage);
                return;
            }

            // Invalid Key.
            _view.TriggerXRayMessage("");
        }

        // data => string.
        void EventTransparencyGroupTrigger_OnActivated(object data)
        {
            bool activated = (data != null);
            string strKey = activated ? (string)data : "";

            var controlData = _context.AnimControllerInfoRef;

            // dumb search again.
            if (!string.IsNullOrEmpty(strKey))
            {
                for (int k = 0; k < controlData.TransparencyList.Count; ++k)
                {
                    if (strKey == controlData.TransparencyList[k].Key)
                    {
                        _view.TriggerTransparencyMessage(controlData.TransparencyList[k].Message);
                        return;
                    }
                }

                // Valid Key but couldn't find data.
                string defaultMessage = "Transparency";
                _view.TriggerTransparencyMessage(defaultMessage);
                return;
            }

            _view.TriggerTransparencyMessage("");
        }

        // data => bool
        void EventTransparencyViewTrigger_OnActivated(object data)
        {
            bool activated = (bool)data;

            string defaultMessage = "Transparency";
            _view.TriggerTransparencyMessage(activated ? defaultMessage : "");
        }
    }
}
