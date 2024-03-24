using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace App.Data
{
    [Serializable]
    public class BundleInfo
    {
        public string Name;
        public string Prefab;
        public int StartFrame = 0;
    }


    // Sugery List Info.
    [Serializable]
    public class SurgeInfo
    {
        public int CPTCode;         // key
        public float RVU;
        public string CategoryId;
        public string SubCategoryId;
        public string Name;
        public string Desc;
        public string IconPath, BigIconPath;
        public string ControllerAsset;
        public List<BundleInfo> BundleDependencies;
    }

}