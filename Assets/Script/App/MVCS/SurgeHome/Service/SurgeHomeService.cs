using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
{
    public class SurgeHomeService : IService
    {
        // [Members] -----------------------------------
        //
        SurgeHomeModel _homeModel;
        SurgeContext _context;
        Action mCallbackFinishedLoading;
        MonoBehaviour mCoroutineOwner;

        public SurgeHomeModel SurgeHomeModel => _homeModel;



        // [Members] -----------------------------------
        //
        public SurgeHomeService(SurgeHomeModel homeMode, SurgeContext context)
        {
            _homeModel = homeMode;
            _context = context;
        }



        // [Public Members] ------------------------------
        //
        public void Init(MonoBehaviour coroutineOwner, Action cbLoadingFinished)
        {
            mCallbackFinishedLoading = cbLoadingFinished;
            mCoroutineOwner = coroutineOwner;

            _homeModel.Reset();

            mCoroutineOwner.StartCoroutine(coInit());
        }





        // [Private Helpers] ------------------------------
        //
        IEnumerator coInit()
        {
            yield return mCoroutineOwner.StartCoroutine(coInitHomeViewData());

            yield return mCoroutineOwner.StartCoroutine(coInitSurgListData());

            if (mCallbackFinishedLoading != null)
                mCallbackFinishedLoading.Invoke();
        }

        IEnumerator coInitHomeViewData()
        {
            if (_homeModel.HomeViewModel != null)
                yield break;

            string homelistConfig = "homeviewlist";
            yield return mCoroutineOwner.StartCoroutine(
                _context.ConfigManager.FetchConfig(homelistConfig, (loadedText) =>
                {
                    if (string.IsNullOrEmpty(loadedText))
                        Debug.LogError($"Loading [{homelistConfig}] has been failed.!");

                    _homeModel.HomeViewModel = JsonUtility.FromJson<SurgeHomeViewModel>(loadedText);

                }));
        }

        IEnumerator coInitSurgListData()
        {
            string surglistConfig = "surgerylist";
            yield return mCoroutineOwner.StartCoroutine(
                _context.ConfigManager.FetchConfig(surglistConfig, (loadedText) =>
                {
                    if (string.IsNullOrEmpty(loadedText))
                        Debug.LogError($"Loading [{surglistConfig}] has been failed.!");

                    _homeModel.SurgeListModel = JsonUtility.FromJson<SurgeListModel>(loadedText);

                }));
        }
    }
}

#region COMMENTS

    /*
    IEnumerator coInitDirector(string directorURL, string directorLocalPath)
    {
        // init local info first. 
        string strJson = Resources.Load<TextAsset>(directorLocalPath).text;
        _homeModel.LocalDirectorModel = JsonUtility.FromJson<SurgeDirectorModel>(strJson);
        Assert.IsTrue(_homeModel.LocalDirectorModel != null);
        
        bool fetching = false;

        // 1. Try fetching Remote data. 
        if (!string.IsNullOrEmpty(directorURL))
        {
            // For this one, we always try to fetch this from remote cloud NOT local cache. 
            fetching = true;
            WWWTextGet.GetDataForTextURLExt(directorURL, (string loadedText, string textUrl) =>
            {
                _homeModel.DirectorModel = JsonUtility.FromJson<SurgeDirectorModel>(loadedText);
                fetching = false;
                PlayerPrefs.SetString("LastValidDirectorInfoPath", directorURL);
                Debug.Log(textUrl + " Downloaded successfully.");
            },
            (string textUrl, string error) =>
            {
                Debug.Log(textUrl + " Downloading has been failed... " + error);
                //var sprite = Resources.Load<Sprite>(FALLBACK_PATH);
                //ImgMainIcon.sprite = sprite;
                fetching = false;
            },
            useCache: false);

            while (fetching)
                yield return null;

            // 2. Try Fetching from cache.
            if (_homeModel.DirectorModel == null)
            {
                directorURL = PlayerPrefs.GetString("LastValidDirectorInfoPath", "");
                if (!string.IsNullOrEmpty(directorURL))
                {
                    fetching = true;
                    WWWTextGet.GetDataForTextURLExt(directorURL, (string loadedText, string textUrl) =>
                    {
                        _homeModel.DirectorModel = JsonUtility.FromJson<SurgeDirectorModel>(loadedText);
                        fetching = false;
                        Debug.Log(textUrl + " Downloaded successfully.");
                    },
                    (string textUrl, string error) =>
                    {
                        Debug.Log(textUrl + " Downloading has been failed... " + error);
                        //var sprite = Resources.Load<Sprite>(FALLBACK_PATH);
                        //ImgMainIcon.sprite = sprite;
                        fetching = false;
                    },
                    useCache: true);

                    while (fetching)
                        yield return null;
                }
            }
        }

        // Have all failed ??
        if (_homeModel.DirectorModel == null)
        {
            Debug.LogWarning("Force fetching local file...couldn't load remote director data...at " + directorURL);

            // We're gonna use localDirectorInfo then.
        }
    }


    // Note - We're trying to fetch these files everytime, but eventually these will be pulled from local caches unless player clears caches. 
    IEnumerator coInitHomeViewData(SurgeDirectorModel remoteDirectorInfo, SurgeDirectorModel localDirectorInfo)
    {
        Assert.IsTrue(localDirectorInfo != null);

        bool fetching = false;
        if (remoteDirectorInfo != null && remoteDirectorInfo.PathList.HomeViewListPath.ToLower().Contains("http"))
        {
            // 1. Try fetching Remote data. 
            fetching = true;
            WWWTextGet.GetDataForTextURLExt(remoteDirectorInfo.PathList.HomeViewListPath, (string loadedText, string textUrl) =>
            {
                _homeModel.HomeViewModel = JsonUtility.FromJson<SurgeHomeViewModel>(loadedText);
                fetching = false;
                Debug.Log(textUrl + " Downloaded successfully.");
            },
            (string textUrl, string error) =>
            {
                Debug.Log(textUrl + " Downloading has been failed... " + error);
                //var sprite = Resources.Load<Sprite>(FALLBACK_PATH);
                //ImgMainIcon.sprite = sprite;
                fetching = false;
            },
            useCache: true);

            while (fetching)
                yield return null;
        }

        // Now, we should try to access local data. 
        if (_homeModel.HomeViewModel == null)
        {
            string strJson = Resources.Load<TextAsset>(localDirectorInfo.PathList.HomeViewListPath).text;
            _homeModel.HomeViewModel = JsonUtility.FromJson<SurgeHomeViewModel>(strJson);
        }

        Assert.IsTrue(_homeModel.HomeViewModel != null);
    }


    // Note - We're trying to fetch these files everytime, but eventually these will be pulled from local caches unless player clears caches. 
    IEnumerator coInitSurgListData(SurgeDirectorModel remoteDirectorInfo, SurgeDirectorModel localDirectorInfo)
    {
        Utils.Assert(localDirectorInfo != null);

        if (remoteDirectorInfo != null && remoteDirectorInfo.PathList.SurgeryListPath.ToLower().Contains("http"))
        {
            // 1. Try fetching Remote data. 
            bool fetching = true;
            WWWTextGet.GetDataForTextURLExt(remoteDirectorInfo.PathList.SurgeryListPath, (string loadedText, string textUrl) =>
            {
                _homeModel.SurgeListModel = JsonUtility.FromJson<SurgeListModel>(loadedText);
                fetching = false;
                Debug.Log(textUrl + " Downloaded successfully.");
            },
            (string textUrl, string error) =>
            {
                Debug.Log(textUrl + " Downloading has been failed... " + error);
                //var sprite = Resources.Load<Sprite>(FALLBACK_PATH);
                //ImgMainIcon.sprite = sprite;
                fetching = false;
            },
            useCache: true);


            while (fetching)
                yield return null;
        }

        // Now we should try to access local data.
        if (_homeModel.SurgeListModel == null)
        {
            string strJson = Resources.Load<TextAsset>(localDirectorInfo.PathList.SurgeryListPath).text;
            _homeModel.SurgeListModel = JsonUtility.FromJson<SurgeListModel>(strJson);
        }

        Assert.IsTrue(_homeModel.SurgeListModel != null);
    }*/
    #endregion
