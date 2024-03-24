using System.Collections;
using UnityEngine;
using Core.Events;
using App.MVCS;
using App.UX;

namespace App
{
    public class DirectorMain : MonoBehaviour
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] TabButtonGroup TabButtons;

        [SerializeField] SurgeLoadingView SurgeLoadingView;
        [SerializeField] SurgeHomeView SurgeHomeView;
        [SerializeField] SurgeAnimationView SurgeAnimationView;
        [SerializeField] PopupDialogView PopupDialogView;

        //const string DIRECTOR_LOCAL_PATH = "Data/director_local";
        EventsGroup Events = new EventsGroup();

        // [Members] ------------------------------
        //
        SurgeContext _context;

        SurgeLoading SurgeLoading;
        SurgeHome SurgeHome;
        SurgeAnimation SurgeAnimation;
        PopupDialog PopupDialog;
        int mSleepTimeout = 0;


        // [Unity Events] ------------------------------
        //
        private void Awake()
        {
#if UNITY_IOS
            SetIOSQuality();
#elif UNITY_ANDROID
            SetAndroidQuality();
#endif
            // Default UI View Target FPS for testing. 
            Application.targetFrameRate = 61;

#if UNITY_EDITOR
            QualitySettings.SetQualityLevel(2);
#elif UNITY_ANDROID
            QualitySettings.vSyncCount = 0;
#endif
        }

        private IEnumerator Start()
        {
            TabButtons.TurnOnTabButton(0);

            _context = new SurgeContext();

            Events.RegisterEvent("SurgeLoading_OnSignInSuccessed", SurgeLoading_OnSignInSuccessed);
            Events.RegisterEvent("OnAnimationAssetBundleFetched", OnAnimationAssetBundleFetched);
            Events.RegisterEvent("OnSignedOut", OnSignedOut);

            SurgeLoading = new SurgeLoading(SurgeLoadingView, _context);
            SurgeLoading.Initialize();

            SurgeHome = new SurgeHome(SurgeHomeView, _context);
            SurgeHome.Initialize();

            SurgeAnimation = new SurgeAnimation(SurgeAnimationView, _context);
            SurgeAnimation.Initialize();

            PopupDialog = new PopupDialog(PopupDialogView, _context);
            PopupDialog.Initialize();

            _context.PopupDialog = PopupDialog;

            mSleepTimeout = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            yield return StartCoroutine(_context.Init(this));
        }

        private void OnApplicationQuit()
        {
            Screen.sleepTimeout = mSleepTimeout;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            else
                Screen.sleepTimeout = mSleepTimeout;
        }



        public void OnBackToHomeViewClicked()
        {
            _context.ShouldInitHomeView = false;

            SurgeLoadingView.gameObject.SetActive(false);
            SurgeHomeView.gameObject.SetActive(true);
            SurgeAnimationView.gameObject.SetActive(false);
        }
        private void SurgeLoading_OnSignInSuccessed(object data)
        {
            _context.IsSignedOut = false;
            _context.ShouldInitHomeView = true;

            SurgeLoadingView.gameObject.SetActive(false);
            SurgeHomeView.gameObject.SetActive(true);
            SurgeAnimationView.gameObject.SetActive(false);
        }
        private void OnSignedOut(object data)
        {
            SurgeLoadingView.gameObject.SetActive(true);
            SurgeHomeView.gameObject.SetActive(false);
            SurgeAnimationView.gameObject.SetActive(false);
        }
    
        private void OnAnimationAssetBundleFetched(object data)
        {
            SurgeLoadingView.gameObject.SetActive(false);
            SurgeHomeView.gameObject.SetActive(false);
            SurgeAnimationView.gameObject.SetActive(true);
        }





        // [Private Helpers] ------------------------------
        //
        void SetIOSQuality()
        {
#if UNITY_IOS
            var iOSGen = UnityEngine.iOS.Device.generation;

            if (Debug.isDebugBuild)
            {
                Debug.Log("iPhone.generation     : " + iOSGen);
                Debug.Log("SystemInfo.deviceType : " + SystemInfo.deviceType);
                Debug.Log("SystemInfo.deviceModel: " + SystemInfo.deviceModel);
            }

            int level = 2;
            if (iOSGen < UnityEngine.iOS.DeviceGeneration.iPhone8)
                level = 0;
            else if (iOSGen <= UnityEngine.iOS.DeviceGeneration.iPhoneX)
                level = 1;

            QualitySettings.SetQualityLevel(level);
            Debug.Log($"Quality Mode initialized to...{level}");
#endif
        }

        void SetAndroidQuality()
        {
            // int cpuCount = SystemInfo.processorCount;
#if UNITY_ANDROID
            int shaderLevel = SystemInfo.graphicsShaderLevel;
            int screenW = Screen.currentResolution.width;
            int screenH = Screen.currentResolution.height;
            int systemMemory = SystemInfo.systemMemorySize;

            if (Debug.isDebugBuild)
            {
                Debug.Log("shader leveln     : " + shaderLevel);
                Debug.Log($"Screen size : {screenW}/{screenH}");
                Debug.Log("System Memory: " + systemMemory);
            }

            int level = 2;
            if (shaderLevel < 20)
                level = 0;
            else if (shaderLevel < 30)
                level = 1;

            QualitySettings.SetQualityLevel( level );
            Debug.Log($"Quality Mode initialized to...{level}");
#endif
        }

    }
}