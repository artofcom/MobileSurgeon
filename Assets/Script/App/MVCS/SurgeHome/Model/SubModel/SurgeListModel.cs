using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using App.Data;

namespace App.MVCS
{
    // Parts Info
    [Serializable]
    public class PartInfo
    {
        public string Name;
        public string IconPath;
    }

    // Category Info.
    [Serializable]
    public class CategoryInfo
    {
        public string Id;
        public List<PartInfo> Parts;         // Shoulder, Elbow, Ankle..etc.
    }


    [Serializable]
    public class SurgeListModel : IModel
    {
        public List<CategoryInfo> CategoryList; // UpperExp, Pelvis, LowerExp
        public List<SurgeInfo> SurgeryList;


        // Utility Access functions.
        public SurgeInfo GetSurgeryInfo(int CPTCode)
        {
            for (int k = 0; k < SurgeryList.Count; ++k)
            {
                if (SurgeryList[k].CPTCode == CPTCode)
                    return SurgeryList[k];
            }
            return null;
        }
        public CategoryInfo GetCategoryInfo(string id)
        {
            for (int k = 0; k < CategoryList.Count; ++k)
            {
                if (CategoryList[k].Id.ToLower() == id.ToLower())
                    return CategoryList[k];
            }
            return null;
        }

        public int GetCategoryCount() => CategoryList.Count;
        public CategoryInfo GetCategoryInfo(int index)
        {
            if (index >= 0 && index < CategoryList.Count)
                return CategoryList[index];
            return null;
        }

        public List<SurgeInfo> GetSurgeInfoFromCategory(string category)
        {
            List<SurgeInfo> listRet = new List<SurgeInfo>();
            for (int k = 0; k < SurgeryList.Count; ++k)
            {
                if (SurgeryList[k].CategoryId.ToLower() == category.ToLower())
                    listRet.Add(SurgeryList[k]);
            }
            return listRet;
        }
        public List<SurgeInfo> GetSurgeInfoFromCategory(string category, string subCategory)
        {
            List<SurgeInfo> listRet = new List<SurgeInfo>();
            for (int k = 0; k < SurgeryList.Count; ++k)
            {
                if (SurgeryList[k].CategoryId.ToLower() == category.ToLower() &&
                    SurgeryList[k].SubCategoryId.ToLower() == subCategory.ToLower())
                    listRet.Add(SurgeryList[k]);
            }
            return listRet;
        }
    }
}
