using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Core.Events;

namespace App.MVCS
{
    public class SurgeAnimationController : IController
    {
        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();

        //  Properties ------------------------------------
        SurgeAnimationModel _model;
        SurgeAnimationView _view;
        SurgeAnimationService _service;
        SurgeContext _context;

        bool mIsAppClosing = false;

        // Thumnail Screenshot Section.
        //int mScreenShotTotalLevel = 10;
        //int mScreenShotProgressLevel = 1;
        //bool mIsTakingScreenShots = false;

        // Sub Controllers --------------------------------
        SurgeTopUIController _topUIController;
        SurgeAnimation3DController _ani3DController;

        // Multi Bundle Player.
        //
        public int AnimBundleIndex { get; set; }
        public SurgeAnimation3DController Ani3DController
        {
            private set { _ani3DController = value; }
            get { return _ani3DController; }
        }

        // Tooling.
        AniPrefabMain mPrefabMain = null;


        //  Methods ---------------------------------------
        //
        public SurgeAnimationController(SurgeAnimationModel model, SurgeAnimationView view, SurgeAnimationService service, SurgeContext context)
        {
            _model = model;
            _view = view;
            _service = service;
            _context = context;

            _topUIController = new SurgeTopUIController(view, model, service, this, context);
            _ani3DController = new SurgeAnimation3DController(view, model, this, context);

            Events.RegisterEvent("SurgeAnimationView_OnEnable", SurgeAnimationView_OnEnable);
            Events.RegisterEvent("SurgeAnimationView_OnDisable", SurgeAnimationView_OnDisable);
            Events.RegisterEvent("SurgeAnimationView_OnApplicationQuit", SurgeAnimationView_OnApplicationQuit);
            Events.RegisterEvent("OnAnimaionMainPrefabLoaded", OnAnimaionMainPrefabLoaded);
            Events.RegisterEvent("OnAnimControllerUpdatedBySelectingPath", OnAnimControllerUpdatedBySelectingPath);


#if UNITY_EDITOR
            Events.RegisterEvent("ViewMain_OnPostCameraRender", ViewMain_OnPostCameraRender);
            Events.RegisterEvent("DebugScreenView_OnTakeScreenShotBtnClicked", DebugScreenView_OnTakeScreenShotBtnClicked);
#endif
        }

        public float GetTotalProgressBarTime()
        {
            var controlData = _context.AnimControllerInfoRef;
            if (controlData != null)
                return (float)_context.Util.FrameToTime(controlData.SectionList[controlData.SectionList.Count - 1].Frame);

            UnityEngine.Assertions.Assert.IsTrue(controlData != null);
            return .0f;
        }

        public float GetProgressBarTime(float curTimeAtTimeLine)
        {
            UnityEngine.Assertions.Assert.IsTrue(_context != null);
            UnityEngine.Assertions.Assert.IsTrue(_context.AnimSurgeInfoRef != null);
            UnityEngine.Assertions.Assert.IsTrue(_context.AnimSurgeInfoRef.BundleDependencies != null);
            UnityEngine.Assertions.Assert.IsTrue(_context.AnimSurgeInfoRef.BundleDependencies.Count > 0);


            //-------------------------------------------------------------------//
            //
            // Previous Multi Bundle finished time check. 
            // 
            float fBundleStartTime = .0f;
            if (AnimBundleIndex >= 0 && AnimBundleIndex < _context.AnimSurgeInfoRef.BundleDependencies.Count)
                fBundleStartTime = (float)_context.Util.FrameToTime(_context.AnimSurgeInfoRef.BundleDependencies[AnimBundleIndex].StartFrame);


            //-------------------------------------------------------------------//
            //
            // Previous Multi Path(TimeLine) finished time check. 
            // 
            float fPlayedTimeLineTime = .0f;
            PlayerablePath pathInfo = _view.SurgeAniView.PrefabMain.GetPlayerablePath(Ani3DController.CurrentPathKey);
            if (pathInfo != null)
            {
                for (int k = 0; k < pathInfo.TimeLine.Length; ++k)
                {
                    if (k < Ani3DController.CurrentPathIndexTimeLine)
                        fPlayedTimeLineTime += (float)pathInfo.TimeLine[k].duration;
                    else break;
                }
            }

            return fBundleStartTime + fPlayedTimeLineTime + curTimeAtTimeLine;
        }




        //  Event Handlers --------------------------------
        //
        private void SurgeAnimationView_OnEnable(object data)
        {
            _view.StartCoroutine(coSurgeAnimationView_OnEnable(data));
        }
        IEnumerator coSurgeAnimationView_OnEnable(object data)
        {
            while (string.IsNullOrEmpty(_context.AnimationBundleName))
                yield return null;

            // Set Target FPS to 30.
            Application.targetFrameRate = 30;

            // ==> Code has been polled into at the end pos of the SurgeSummaryControl.
            //
            // string strDataPath = _context.BootStrap.setting.UseRemoteBundle ? "main.json" : "data/main.json";
            // yield return _view.StartCoroutine(_service.LoadSurgeAnimationInfo(_view, _context.AnimationBundleName, strDataPath));
            //

            _view.StartAnimationView(_context.BootStrap.userData.ExpertMode, _context.AnimControllerInfoRef.TitleMessage);

            _topUIController.Start(AnimIndex: 0);

            _ani3DController.Start(_context.AnimationBundlePrefab, indexAnim: 0, startAniTimeAtBundle: 0.0f, _context.AnimControllerInfoRef.StartingDelayInSecond);
        }
        private void SurgeAnimationView_OnDisable(object data)
        {
            if (!mIsAppClosing)
                _view.LeaveAnimationView();

            // Set Target FPS back to 60.
            Application.targetFrameRate = 61;

            // mIsTakingScreenShots = false;
            AnimBundleIndex = -1;
        }
        private void SurgeAnimationView_OnApplicationQuit(object data)
        {
            mIsAppClosing = true;
        }
        void OnAnimaionMainPrefabLoaded(object data)
        {
            var prefabMain = (GameObject)data;
            if (prefabMain != null)
            {
                mPrefabMain = prefabMain.GetComponent<AniPrefabMain>();
            }
        }
        void OnAnimControllerUpdatedBySelectingPath(object data)
        {
            // Refresh Top UI.
            _topUIController.Start(AnimIndex: 0);
        }


        #region === THUMBNAIL PREVIEW HANDLER

        //  Thumbnail screen shot helpers --------------------------------
        //
        //
#if UNITY_EDITOR
        void DebugScreenView_OnTakeScreenShotBtnClicked(object data)
        {
            /*
            mIsTakingScreenShots = true;
            mScreenShotProgressLevel = 0;

            float rate = .0f;
            EventSystem.DispatchEvent("OnProgressBarDragged", (object)rate);
            */
        }
        void ViewMain_OnPostCameraRender(object data)
        {
            /*
            if (!mIsTakingScreenShots) return;

            Camera cam = (Camera)data;
            // Has valid set of last camera info. -> Then check if that's the one.
            if (mPrefabMain != null && mPrefabMain.LastRenderedCamera != null)
            {
                if (mPrefabMain.LastRenderedCamera != cam)
                    return;
            }
            // else -> check the 'no post effect cam'.
            else
            {
                if (cam.transform.parent == null)
                    return;
                if (!cam.transform.parent.gameObject.name.ToLower().Contains("noposteffect"))
                    return;
            }

            mScreenShotTotalLevel = mPrefabMain!=null ? mPrefabMain.ThumbnailCount : mScreenShotTotalLevel;

            // Taking Screen shots.
            int w = Screen.width;
            int h = Screen.height;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            tex.ReadPixels(rect, 0, 0, false);
            tex.Apply();
            tex = Scaled(tex, 128, 128);


            string buildPath = Application.dataPath + $"/Bundles/{_context.AnimationBundleName}/thumbnails";
            if (!System.IO.Directory.Exists(buildPath))
                System.IO.Directory.CreateDirectory(buildPath);

            byte[] byteArray = tex.EncodeToJPG();
            System.IO.File.WriteAllBytes($"{buildPath}/shot_{mScreenShotProgressLevel}.jpg", byteArray);  // 0 ~ total level.

            // total mScreenShotTotalLevel + 1 pictures.
            ++mScreenShotProgressLevel;
            if (mScreenShotProgressLevel > mScreenShotTotalLevel)
                mIsTakingScreenShots = false;
            else
            {
                float oneLevelRate = 1.0f / ((float)(mScreenShotTotalLevel));

                float nextRate = oneLevelRate * mScreenShotProgressLevel;
                EventSystem.DispatchEvent("OnProgressBarDragged", (object)nextRate);
            }*/
        }

        public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }

        // Internal unility that renders the source texture into the RTT - the scaling method itself.
        private static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
#endif

        #endregion
    }

}