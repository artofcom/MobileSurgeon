using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
{
    // Card Info.
    [Serializable]
    public class HomeCardInfo
    {
        public string Type;
        public int CPTCode;
        public string Tag;
        public string IconPath;
        public string Desc;
    }


    [Serializable]
    public class SurgeHomeViewModel : IModel
    {
        public List<HomeCardInfo> CardList;
    }
}
