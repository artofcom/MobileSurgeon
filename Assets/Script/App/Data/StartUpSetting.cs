using System;

namespace App.Data
{
    [Serializable]
    public class StartUpSettingInfo
    {
        public bool UseRemoteConfig, UseRemoteBundle;
        public string CDNProviderHeader;
    }
}