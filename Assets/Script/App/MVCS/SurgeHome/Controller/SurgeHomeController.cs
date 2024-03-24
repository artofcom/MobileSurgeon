using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using UnityEngine.Assertions;
using App.Data;
using App.Manager.Data;

namespace App.MVCS
{
    public class SurgeHomeController : IController
    {
        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        //  Properties ------------------------------------
        SurgeHomeModel _homeModel;
        SurgeHomeView _view;
        SurgeHomeService _service;
        SurgeContext _context;

        // Sub Controllers --------------------------------
        HomeTabController _homeTabController;
        SpecialtyTabController _specialtyTabController;
        CPTTabController _CPTTabController;
        OptionTabController _optionTabController;


        //  Methods ---------------------------------------
        public SurgeHomeController(SurgeHomeModel homeModel, SurgeHomeView view, SurgeHomeService service, SurgeContext context)
        {
            _homeModel = homeModel;
            _view = view;
            _service = service;
            _context = context;


            _homeTabController = new HomeTabController(view, service, context);
            _specialtyTabController = new SpecialtyTabController(view, homeModel, context);
            _CPTTabController = new CPTTabController(view, homeModel, context);
            _optionTabController = new OptionTabController(view, context);


            Events.RegisterEvent("SurgeHomeView_OnEnable", SurgeHomeView_OnEnable);
            Events.RegisterEvent("OnSurgItemClicked", OnSurgItemClicked);
        }


        //  Event Handlers ----------------------------------
        void SurgeHomeView_OnEnable(object data)
        {
            _context.SummaryInfoFileName = string.Empty;

            if (_context.ShouldInitHomeView)
                _view.InitView();
        }

        void OnSurgItemClicked(object data)
        {
            _view.StartCoroutine(CoLoadBundle(CPTCode: (int)data));
        }

        IEnumerator CoLoadBundle(int CPTCode)
        {
            var SurgeInfo = _service.SurgeHomeModel.GetSurgeInfo(CPTCode);
            Assert.IsTrue(SurgeInfo != null);
            if (SurgeInfo == null)
                yield break;

            // Easy Access Caching.
            _context.AnimSurgeInfoRef = SurgeInfo;

            _context.AnimControllerInfoRef = null;
            _context.AnimBranchControllerInfoRef = null;


            yield return _view.StartCoroutine(coLoadController(SurgeInfo));

            yield return _view.StartCoroutine(coLoadBranchSystem());

            yield return _view.StartCoroutine(coLoadPrefab(SurgeInfo));


            // All is done.
            const string SCENE_TRANSITION_EVENT = "OnAnimationAssetBundleFetched";
            EventSystem.DispatchEvent(SCENE_TRANSITION_EVENT);
        }


        IEnumerator coLoadController(App.Data.SurgeInfo SurgeInfo)
        {
            // Load Controller config.
            string controllerName = SurgeInfo.ControllerAsset;
            if (!_context.BootStrap.setting.UseRemoteBundle)
                controllerName += ".json";

            bool isFinishLoadingConfig = false;
            yield return _view.StartCoroutine(_context.AnimCtrlFetcher.CoLoadController(controllerName,
                (loadedInfo) =>
                {
                    SurgeControlInfo controlInfo = loadedInfo;

                    // Has valid BranchSystem ?
                    if (controlInfo.BranchSystem != null && controlInfo.BranchSystem.PathInfoList != null && controlInfo.BranchSystem.PathInfoList.Count > 0)
                        _context.AnimBranchControllerInfoRef = loadedInfo;

                    else
                        _context.AnimControllerInfoRef = loadedInfo;

                    isFinishLoadingConfig = true;
                }));

            yield return new WaitUntil(() => isFinishLoadingConfig == true);
        }


        // Load and Cache all the controler info for the anim-path controllers.
        IEnumerator coLoadBranchSystem()
        {
            if (_context.AnimBranchControllerInfoRef == null || _context.AnimBranchControllerInfoRef.BranchSystem == null)
                yield break;

            _context.PathKeyControllerListInfoRef.Clear();

            BranchSystemInfo branchSysInfo = _context.AnimBranchControllerInfoRef.BranchSystem;

            // Also need to load sub Path-configs.
            for (int k = 0; k < branchSysInfo.PathInfoList.Count; ++k)
            {
                bool isFinishLoadingConfig = false;
                string controllerFile = branchSysInfo.PathInfoList[k].ControllerName;
                if (!_context.BootStrap.setting.UseRemoteBundle)
                    controllerFile += ".json";
                yield return _view.StartCoroutine(_context.AnimCtrlFetcher.CoLoadController(controllerFile,
                    (loadedInfo) =>
                    {
                        Assert.IsTrue(loadedInfo != null);

                        // Cache key and Controller Info.
                        _context.PathKeyControllerListInfoRef.Add(
                            new System.Tuple<string, SurgeControlInfo>(
                                branchSysInfo.PathInfoList[k].PathKey,  // Path Key.
                                loadedInfo)                             // Controller Data.    
                        );

                        isFinishLoadingConfig = true;
                    }));

                yield return new WaitUntil(() => isFinishLoadingConfig == true);
            }

            // Update the ACTIVE one for starting.
            if (branchSysInfo.StartingPathIndex >= 0 && branchSysInfo.StartingPathIndex < _context.PathKeyControllerListInfoRef.Count)
                _context.AnimControllerInfoRef = _context.PathKeyControllerListInfoRef[branchSysInfo.StartingPathIndex].Item2;
            else
                Assert.IsTrue(false, $"BranchSystem) Invalid String Path Index : {branchSysInfo.StartingPathIndex} !");
        }



        IEnumerator coLoadPrefab(App.Data.SurgeInfo SurgeInfo)
        {
            const string BUNDLE_DOWNLOAD_PROG_EVENT = "OnAssetBundleDownloadProgress";

            _context.AnimationBundleName = SurgeInfo.BundleDependencies[0].Name;
            Debug.Log($"Setting context.AnimationBundleName to [{_context.AnimationBundleName}]!");


            // Asset Download Dialog Display =====================================
            // 
            AssetBundle animBundle = null;
            bool IsBundleCached = _context.ABManager.IsCached(_context.AnimationBundleName);
            bool IsDownloadingAsset = false;
            if (_context.BootStrap.setting.UseRemoteBundle)
            {
                if (!IsBundleCached)
                {
                    IsDownloadingAsset = true;

                    // Make sure to clear all the old version of this bundle.
                    _context.ABManager.ClearCache(_context.AnimationBundleName);

                    // Try Download AssetBundle.
                    yield return _view.StartCoroutine(_context.CoDownloadBundle(_context.AnimationBundleName,

                            (downloadBundle) =>
                            {
                                animBundle = downloadBundle;
                                EventSystem.DispatchEvent(BUNDLE_DOWNLOAD_PROG_EVENT, (object)1.0f);    // Should make the dialog closed. 
                            },
                            (fProgress) =>
                            {
                                EventSystem.DispatchEvent(BUNDLE_DOWNLOAD_PROG_EVENT, (object)fProgress);
                            })
                        );


                    // Launch Download Dialog.
                    var presentData = new BundleDownloadDialogView.PresentData();
                    presentData.BundleName = _context.AnimationBundleName;
                    _context.TriggerDialog("assetDownloadDialog", presentData, (returnedValue) =>
                    {
                        var ret = returnedValue as BundleDownloadDialogView.ReturnData;
                        if (ret.ok)
                            IsDownloadingAsset = false;
                        else
                        {
                            // eLoadingState = ELOADING_STATE.ELOADING_FAILED;
                            // isAborted = true;
                            _context.ABManager.AbortDownloading(presentData.BundleName);
                        }
                    });
                }
                else
                {
                    IsDownloadingAsset = true;
                    yield return _view.StartCoroutine(_context.CoDownloadBundle(_context.AnimationBundleName,
                        (downloadBundle) =>
                        {
                            animBundle = downloadBundle;
                            IsDownloadingAsset = false;

                        }, null));
                }
            }

            yield return new WaitUntil(() => (IsDownloadingAsset == false));
            if (_context.BootStrap.setting.UseRemoteBundle && animBundle == null)
            {
                Assert.IsTrue(animBundle != null);
                yield break;
            }




            // Load Main Prefab Asset =====================================
            // 
            string prefabName = SurgeInfo.BundleDependencies[0].Prefab;
            prefabName = _context.BootStrap.setting.UseRemoteBundle ? prefabName : prefabName + ".prefab";
            yield return _view.StartCoroutine(_context.CoLoadAssetFromBundle(animBundle, prefabName, _context.AnimationBundleName));
            Assert.IsTrue(_context.AnimationBundlePrefab != null);
        }
    }
}
