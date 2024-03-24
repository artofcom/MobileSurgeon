using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AssetBundle;
using App.Manager.Data;
using System;
using App.Data;
using App.Common;
using App.Manager;

namespace App.MVCS
{
    public class SurgeContext
    {
        public BootStrap BootStrap { get; private set; }
        public AssetBundleManager ABManager { get; private set; }
        public ConfigManager ConfigManager { get; private set; }
        public PopupDialog PopupDialog { private get; set; }
        public AnimControllerDataFetcher AnimCtrlFetcher { get; private set; }
        public Util Util { get; private set; }


        public string SummaryInfoFileName { get; set; }
        public string AnimationBundleName { get; set; }
        public GameObject AnimationBundlePrefab { get; set; }
        public bool Initialized { get; private set; } = false;
        public bool IsSignedOut { get; set; } = false;
        public bool ShouldInitHomeView { get; set; } = true;

        // public List<SurgeInfo> SurgeryListRef { get; set; }

        public SurgeControlInfo AnimControllerInfoRef { get; set; }

        // Only Valid if for Branch System.
        public SurgeControlInfo AnimBranchControllerInfoRef { get; set; }
        public List<Tuple<string, SurgeControlInfo>> PathKeyControllerListInfoRef { get; set; } = new List<Tuple<string, SurgeControlInfo>>();
        //

        public SurgeInfo AnimSurgeInfoRef { get; set; }

        MonoBehaviour mCoroutineOwner;

        //
        public IEnumerator Init(MonoBehaviour monoObject)
        {
            mCoroutineOwner = monoObject;

            BootStrap = new BootStrap();
            ConfigManager = new ConfigManager();
            ABManager = new AssetBundleManager();
            AnimCtrlFetcher = new AnimControllerDataFetcher();
            Util = new App.Common.Util();

            Debug.Log("Context Init Started.");

            yield return mCoroutineOwner.StartCoroutine(BootStrap.Init());

            yield return mCoroutineOwner.StartCoroutine(ConfigManager.Init(monoObject, BootStrap.setting.UseRemoteConfig, BootStrap.setting.CDNProviderHeader));

            yield return mCoroutineOwner.StartCoroutine(ABManager.Init(monoObject, BootStrap.setting.UseRemoteBundle, BootStrap.setting.CDNProviderHeader));

            AnimCtrlFetcher.Init(mCoroutineOwner, ABManager, BootStrap.setting.UseRemoteBundle);

            Initialized = true;

            Debug.Log("Context Init Done.");
        }


        // Dialog Control.
        //
        public void TriggerDialog(string strDialogName, IDialogPresentData data, Action<IDialogReturn> callbackDone)
        {
            PopupDialog.TriggerDialog(strDialogName, data, callbackDone);
        }
        public void CloseDialog(string strDialogName = "")
        {
            PopupDialog.CloseDialog(strDialogName);
        }



        //
        // TEMP Code, since we know we not gonna have the Summary View.
        //

        //public IEnumerator CoLoadAnimController(string ctrlName)
        //{
        //    yield return mCoroutineOwner.StartCoroutine(AnimCtrlFetcher.CoLoadController(ctrlName, null));
        //}

        public IEnumerator CoDownloadBundle(string bundleName, Action<AssetBundle> callbackDone, Action<float> callbackProgress)
        {
            yield return mCoroutineOwner.StartCoroutine(ABManager.FetchBundle(bundleName, callbackDone, callbackProgress));
        }

        public IEnumerator CoLoadAssetFromBundle(AssetBundle animBundle, string assetName, string bundleNameForOffline = "")
        {
            if (!BootStrap.setting.UseRemoteBundle && !string.IsNullOrEmpty(bundleNameForOffline))
            {
                yield return mCoroutineOwner.StartCoroutine(
                    ABManager.LoadAssetBundle<GameObject>(bundleNameForOffline, assetName,
                    (loadedPrefab) =>
                    {
                        AnimationBundlePrefab = loadedPrefab;
                    },
                    (progress) => { }
                    ));
            }
            else
            {
                yield return mCoroutineOwner.StartCoroutine(ABManager.LoadAssetFromBundle<GameObject>(animBundle, assetName,
                    (loadedObject) =>
                    {
                        AnimationBundlePrefab = loadedObject;

                    }));
            }
        }
    }
}
