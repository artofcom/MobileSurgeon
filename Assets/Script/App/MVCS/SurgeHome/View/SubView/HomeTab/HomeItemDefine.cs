using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.MVCS
{
    public class HomeItemDefine
    {
        public class PresentModel
        {
            public int CPTCode;
            public string Type, ImagePath, Desc, Tag;
            public float RVU;
            public bool CanRemoveFromBookmark;
        }
    }
}