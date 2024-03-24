using System.Collections.Generic;
using System;

namespace App.Manager.Data
{
    [Serializable]
    public class HeaderInfo
    {
        public string date;
    }

    [Serializable]
    public class ConfigInfo
    {
        public string name, path;
    }

    [Serializable]
    public class ManifestInfo
    {
        public HeaderInfo header;
        public List<ConfigInfo> configs;

        public ConfigInfo GetConfigInfo(string strName)
        {
            for(int k = 0; k < configs.Count; ++k)
            {
                if (configs[k].name == strName)
                    return configs[k];
            }
            return null;
        }
    }
}
