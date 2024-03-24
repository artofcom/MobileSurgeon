using System.Collections.Generic;
using System;

namespace App.Manager.Data
{
    [Serializable]
    public class SectionInfo
    {
        public uint Frame;
        public string Message;
    }

    [Serializable]
    public class GroupInfo
    {
        public uint Frame;
        public string Name;
    }

    [Serializable]
    public class MessageInfo
    {
        public string Key;
        public string Message;
    }


    [Serializable]
    public class XRayInfo
    {
        public string Key;
        public string Message;
    }


    [Serializable]
    public class TransparencyInfo
    {
        public string Key;
        public string Message;
    }


    [Serializable]
    public class PreviewThumbnailInfo
    {
        public string ImageTemplate;
        public string Path;
        public int ImageCount;
    }


    // Branch System Related. =======================================
    //
    [Serializable]
    public class AnimPathInfo
    {
        public string ControllerName;
        public string PathKey;
    }

    [Serializable]
    public class BranchSystemInfo
    {
        public List<AnimPathInfo> PathInfoList;
        public int StartingPathIndex;
    }



    [Serializable]
    public class SurgeControlInfo
    {
        public string Version;
        public float StartingDelayInSecond;
        public string CPTCode;
        public string TitleMessage;
        public List<SectionInfo> SectionList;
        public List<GroupInfo> GroupList;
        public List<MessageInfo> MessageList;
        public List<XRayInfo> XRayList;
        public List<TransparencyInfo> TransparencyList;
        public PreviewThumbnailInfo PreviewThumbnail;

        public BranchSystemInfo BranchSystem;
    }

}