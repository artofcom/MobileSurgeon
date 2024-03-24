using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using App.Manager.Data;
using Core.WWW;

namespace App.Manager
{
    public class ConfigManager
    {
        // [const values] ------------------------------
        //
        // const string ResourcesAssetBasePath = "Assets/Bundles/SurgeryAnimations";
        const string LastValidURLKey = "LastValidConfigURL";
        const string LOCAL_CONFIG_PATH = "Local/configs/manifest/configManifest";


        // [Members] -----------------------------------
        //
        ManifestInfo mManifestInfo, mLocalManifestInfo;
        Dictionary<string, string> ConfigCache = new Dictionary<string, string>();
        bool mUseRemoteConfig = true;
        MonoBehaviour mCoroutineRunner;
        string mCDNHead = string.Empty;


        // [Public functions] ---------------------
        //
        public IEnumerator Init(MonoBehaviour coroutineRunner, bool useRemoteConfig, string CDNHead)
        {
            mCoroutineRunner = coroutineRunner;
            mUseRemoteConfig = useRemoteConfig;
            mCDNHead = CDNHead;
#if !UNITY_EDITOR
            mUseRemoteConfig = true;
#endif
            string strJson = Resources.Load<TextAsset>(LOCAL_CONFIG_PATH).text;
            if(!string.IsNullOrEmpty( strJson ))
                mLocalManifestInfo = JsonUtility.FromJson< ManifestInfo >(strJson);
            UnityEngine.Assertions.Assert.IsTrue(mLocalManifestInfo != null);

            if (false == mUseRemoteConfig)      // Is this offline mode?
                yield break;
            
            // Remote.
            DateTime dateTime = DateTime.UtcNow;
            string cacheIgnoreKey = $"?cacheignorekey={dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Second}";
            string manifestURL = $"{CDNHead}/configs/manifest/configmanifest_{Application.version}.json" + cacheIgnoreKey;
            bool fetching = true;
            WWWTextGet.GetDataForTextURLExt(manifestURL, (string loadedText, string textUrl) =>
            {
                if(!string.IsNullOrEmpty(loadedText))
                    mManifestInfo = JsonUtility.FromJson<ManifestInfo>(loadedText);
                if(mManifestInfo != null)
                    PlayerPrefs.SetString(LastValidURLKey, manifestURL);

                fetching = false;
                if (mManifestInfo != null)  Debug.Log(textUrl + " Downloaded successfully.");
                else                        Debug.LogWarning(textUrl + "Download has been failed.");
            },
            (string textUrl, string error) =>
            {
                Debug.LogWarning(textUrl + " Downloading has been failed... " + error);
                fetching = false;
            },
            useCache: false);

            while (fetching)
                yield return null;

            // Local cache. => Try get the config from the last recent valid one from cache.
            if(mManifestInfo == null)
            {
                manifestURL = PlayerPrefs.GetString(LastValidURLKey, "");
                if(!string.IsNullOrEmpty(manifestURL))
                {
                    fetching = true;
                    WWWTextGet.GetDataForTextURLExt(manifestURL, (string loadedText, string textUrl) =>
                    {
                        if (!string.IsNullOrEmpty(loadedText))
                            mManifestInfo = JsonUtility.FromJson<ManifestInfo>(loadedText);
                        fetching = false;
                        if(mManifestInfo != null)   Debug.Log(textUrl + " loaded from cache successfully.");
                        else                        Debug.LogWarning(textUrl + "Download from cache has been failed.");
                    },
                    (string textUrl, string error) =>
                    {
                        Debug.LogWarning(textUrl + " has been failed to load from cache... " + error);
                        fetching = false;
                    },
                    useCache: true);

                    while (fetching)
                        yield return null;
                }
            }

            // Fallback.
            if(mManifestInfo == null)
            {
                strJson = Resources.Load<TextAsset>(LOCAL_CONFIG_PATH).text;
                if (!string.IsNullOrEmpty(strJson))
                    mManifestInfo = JsonUtility.FromJson<ManifestInfo>(strJson);
            }

            UnityEngine.Assertions.Assert.IsTrue(mManifestInfo != null);
        }

        public IEnumerator FetchConfig(string configName, Action<string> callbackDone) //where T : UnityEngine.Object
        {
            configName = configName.ToLower();

            string strLoadedJson = string.Empty;

            // Try fetch data from Remote.
            ConfigInfo configInfo = null;
            if (mUseRemoteConfig)
            {
                if (mManifestInfo == null || mManifestInfo.configs == null)
                {
                    Debug.LogError("[Config] Invalid manifest Info !");
                    if (callbackDone != null)
                        callbackDone(null);
                    yield break;
                }

                configInfo = mManifestInfo.GetConfigInfo(configName);
                if (configInfo == null)
                {
                    Debug.LogError("[Config] Couldn't find the config... " + configName);
                    if (callbackDone != null)
                        callbackDone(null);
                    yield break;
                }
            }
            // Local Mode.
            else    
            {
                strLoadedJson = LoadConfigFromLocal(configName);
                if(string.IsNullOrEmpty(strLoadedJson))
                    Debug.LogError("[Config] Couldn't find the config... " + configName);

                if (callbackDone != null)
                    callbackDone(strLoadedJson);
                
                yield break;
            }

            
            string dataPath = $"{mCDNHead}/{configInfo.path}/{configInfo.name}.json";
            if(ConfigCache.ContainsKey(dataPath))
            {
                if (callbackDone != null)
                    callbackDone( ConfigCache[dataPath] );
                yield break;
            }

            
            bool fetching = true;
            WWWTextGet.GetDataForTextURLExt(dataPath, (string loadedText, string textUrl) =>
            {
                strLoadedJson = loadedText;
                if (!string.IsNullOrEmpty(strLoadedJson))
                    ConfigCache[dataPath] = strLoadedJson;

                fetching = false;
                Debug.Log(textUrl + " Downloaded successfully.");
            },
            (string textUrl, string error) =>
            {
                Debug.Log(textUrl + " Downloading has been failed... " + error);
                fetching = false;
            },
            useCache: true);

            while (fetching)
                yield return null;


            // Fallback.
            if(string.IsNullOrEmpty(strLoadedJson))
            {
                strLoadedJson = LoadConfigFromLocal(configName);
                if (string.IsNullOrEmpty(strLoadedJson))
                    Debug.LogError("[Config] Couldn't find the config... " + configName);
            }


            if (callbackDone != null)
                callbackDone( strLoadedJson );
        }

        public void ClearCache()
        {
            ConfigCache.Clear();
        }

        public bool IsCached(string configName)
        {
            if (string.IsNullOrEmpty(configName))
                return false;

            if (mManifestInfo == null || mManifestInfo.configs == null)
                return false;

            ConfigInfo configInfo = mManifestInfo.GetConfigInfo(configName);
            if (configInfo == null)
                return false;

#if UNITY_EDITOR
            // Local Mode.
            if (mUseRemoteConfig == false)
            {
                configInfo = mLocalManifestInfo.GetConfigInfo(configName);
                string assetPath = $"{configInfo.path}/{configName}";
                return ConfigCache.ContainsKey(assetPath);
            }
#endif
            
            string dataPath = $"{mCDNHead}/{configInfo.path}/{configInfo.name}.json";
            return ConfigCache.ContainsKey( dataPath );
        }






        string LoadConfigFromLocal(string configName)
        {
            ConfigInfo configInfo = mLocalManifestInfo.GetConfigInfo(configName);
            if (configInfo == null)
                return string.Empty;
            
            // Load Bundle from Local location.
            string assetPath = $"{configInfo.path}/{configName}";
            Debug.Log("[Config] Loading locally..." + assetPath);

            string strJson;
            if (ConfigCache.ContainsKey(assetPath))
                strJson = ConfigCache[assetPath];
            else
                strJson = Resources.Load<TextAsset>(assetPath).text;

            ConfigCache[assetPath] = strJson;
            return strJson;
        }
    }
}


/*
 *public IEnumerator GetAssetBundle(string name, string prefabName, Action<GameObject> callbackDone)
        {
            if (manifestInfo == null || manifestInfo.bundles == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            name = name.ToLower();


            UnityEngine.AssetBundle bundle = null;
            if (BundleCache.ContainsKey(name))
            {
                Debug.Log("[Fetching] Loading Bundle From Runtime Cache..." + name);
                bundle = BundleCache[name];
                var _loadAsset = bundle.LoadAssetAsync<GameObject>(prefabName);
                yield return _loadAsset;

                var _loadedObject = (GameObject)_loadAsset.asset;
                if (callbackDone != null)
                    callbackDone.Invoke(_loadedObject);

                yield break;
            }

            
            BundleInfo info = manifestInfo.GetBundleInfo(name);
            if (info == null)
            {
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            if (IsCached(name))
                Debug.Log("[Fetching] Loading Bundle From Local Cache..." + name);
            else
                Debug.Log("[Fetching] Downloading Bundle From CDN..." + name);


            string URLHead = "https://storage.googleapis.com/";
            string path = URLHead + info.path;
            path += "/";
            path += info.file;

            var www = UnityWebRequestAssetBundle.GetAssetBundle( path, Hash128.Parse(info.hash) );
            www.SendWebRequest();
            while (www.downloadProgress < 1.0f)
            {
                Debug.Log(www.downloadProgress);
                yield return null;
            }
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                if (callbackDone != null)
                    callbackDone.Invoke(null);
                yield break;
            }

            bundle = DownloadHandlerAssetBundle.GetContent(www);
            if (!BundleCache.ContainsKey(name))
                BundleCache[name] = bundle;

            var loadAsset = bundle.LoadAssetAsync<GameObject>(prefabName);
            yield return loadAsset;

            var loadedObject = (GameObject)loadAsset.asset;
            if (callbackDone != null)
                callbackDone.Invoke(loadedObject);
        }
 */