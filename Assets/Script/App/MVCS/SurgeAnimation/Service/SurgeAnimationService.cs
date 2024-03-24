using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
    {
    public class SurgeAnimationService : IService
    {
        SurgeAnimationModel _model;
        SurgeContext _context;

        public SurgeAnimationService(SurgeAnimationModel model, SurgeContext context)
        {
            _model = model;
            _context = context;
        }

        public IEnumerator LoadThumnailImage(MonoBehaviour coRunner, string bundleName, string imgName, Action<Sprite> callback)
        {
            string strBundleName = bundleName;      // "003_eyesurgery"
            bool loaded = false;                    // "hash": "e26914bd4d862dc9574eb0a8f28d6079",
            coRunner.StartCoroutine(
                _context.ABManager.LoadAssetBundle<Sprite>(strBundleName, imgName,
                (loadedSprite) =>
                {
                    //if (loadedText != null && !string.IsNullOrEmpty(loadedText.text))
                    //    _model.SurgeAniInfo = JsonUtility.FromJson<SurgeMainInfo>(loadedText.text);

                    if (callback != null)
                        callback.Invoke(loadedSprite);

                    loaded = true;
                }, null));

            while (!loaded)
                yield return null;

            yield break;
        }



        public void DownloadBundles(MonoBehaviour coRunner, List<string> bundleNames,
            Action<AssetBundle> callbackDone, Action<float> callbackDownloading)
        {
            coRunner.StartCoroutine(coDownloadAssets(coRunner, bundleNames, callbackDone, callbackDownloading));
        }

        IEnumerator coDownloadAssets(MonoBehaviour coRunner, List<string> bundleNames,
            Action<AssetBundle> callbackDone, Action<float> callbackDownloading)
        {
            for (int k = 0; k < bundleNames.Count; ++k)
            {
                string bundleName = bundleNames[k];

                _context.ABManager.ClearCache(bundleName);

                yield return coRunner.StartCoroutine(_context.CoDownloadBundle(bundleName,
                    callbackDone, callbackDownloading));
            }
        }
    }

}
