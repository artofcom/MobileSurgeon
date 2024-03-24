using System;


namespace App.MVCS
{
    // Path Info.
    [Serializable]
    public class DataPathInfo
    {
        public string HomeViewListPath;
        public string SurgeryListPath;
    }

    // Debug Info.
    [Serializable]
    public class DebugInfo
    {
        public bool Online;
        public string tag;
    }

    [Serializable]
    public class SurgeDirectorModel : IModel
    {
        public DataPathInfo PathList;
        public DebugInfo DebugInfo;
    }

}