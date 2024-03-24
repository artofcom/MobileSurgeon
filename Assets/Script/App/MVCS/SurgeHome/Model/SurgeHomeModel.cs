using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
{
    public class SurgeHomeModel : IModel
    {
        public SurgeDirectorModel DirectorModel { get; set; }
        public SurgeDirectorModel LocalDirectorModel { get; set; }
        public SurgeHomeViewModel HomeViewModel { get; set; }
        public SurgeListModel SurgeListModel { get; set; }

        public void Reset()
        {
            LocalDirectorModel = null;
            DirectorModel = null;
            HomeViewModel = null;
            SurgeListModel = null;
        }

        public App.Data.SurgeInfo GetSurgeInfo(int CPTCode)
        {
            if (SurgeListModel.SurgeryList == null)
                return null;

            int ret = SurgeListModel.SurgeryList.FindIndex(x => x.CPTCode == CPTCode);
            if (ret >= 0 && ret < SurgeListModel.SurgeryList.Count)
                return SurgeListModel.SurgeryList[ret];

            return null;
        }
    }
}
