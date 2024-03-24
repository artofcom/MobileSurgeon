using System.Collections;
///using System.Collections.Generic;
using UnityEngine;
///using UnityEngine.Networking;
using App.Data;

namespace App.Common
{
    public class BootStrap
    {
        // [Const values] ------------------------------
        //
        const string LOCAL_CONFIG_PATH = "Local/configs/boot";


        // [Member Values] ------------------------------
        //
        public UserDataHolder userData { get; private set; }
        public StartUpSettingInfo setting { get; private set; }


        // [public members] -----------------------------
        //
        public IEnumerator Init()
        {
            // should init only once.
            if (userData != null) yield break;

            userData = new UserDataHolder();
            userData.Init();

            string strJson = Resources.Load<TextAsset>(LOCAL_CONFIG_PATH).text;
            setting = JsonUtility.FromJson<StartUpSettingInfo>(strJson);

#if !UNITY_EDITOR
            setting.UseRemoteConfig = true;
            setting.UseRemoteBundle = true;
#endif

            UnityEngine.Assertions.Assert.IsTrue(setting != null);
            yield return null;
        }
    }
}
