using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using App.Data;

namespace App.MVCS
{
    public class SpecialtyTabController
    {
        //  Events ----------------------------------------
        //
        EventsGroup Events = new EventsGroup();


        //  Property --------------------------------------
        //
        SurgeHomeView _view;
        SurgeHomeModel _model;
        SurgeContext _context;
        string mCategoryKey, mLastSubCategoryKey;
        bool mOneTimeInit = false;

        //  Methods ----------------------------------------
        //
        public SpecialtyTabController(SurgeHomeView view, SurgeHomeModel model, SurgeContext context)
        {
            _view = view;
            _model = model;
            _context = context;


            Events.RegisterEvent("SpecialtyTabView_OnEnabled", SpecialtyTabView_OnEnabled);
            Events.RegisterEvent("OnSpecialtyCategoryItemClicked", OnSpecialtyCategoryItemClicked);
            Events.RegisterEvent("OnTabCategoryClicked", OnTabCategoryClicked);
            Events.RegisterEvent("OnSpecialtyTabBackBtnClicked", OnSpecialtyTabBackBtnClicked);

            Events.RegisterEvent("SpecialtySurgeListView_OnEnabled", SpecialtySurgeListView_OnEnabled);
            Events.RegisterEvent("SpecialtySurgeListView_OnDisabled", SpecialtySurgeListView_OnDisabled);
        }





        //  Event Handlers ----------------------------------------
        //
        void SpecialtyTabView_OnEnabled(object data)
        {
            if (mOneTimeInit) return;

            _view.StartCoroutine(CoInit());
            mOneTimeInit = true;
        }
        void SpecialtySurgeListView_OnEnabled(object data)
        {
            Events.RegisterEvent("SurgListItemView_OnBtnDeleteCacheClicked", SurgListItemView_OnBtnDeleteCacheClicked);

            if (!string.IsNullOrEmpty(mLastSubCategoryKey))
                OnSpecialtyCategoryItemClicked(null);
        }
        void SpecialtySurgeListView_OnDisabled(object data)
        {
            Events.UnRegisterEvent("SurgListItemView_OnBtnDeleteCacheClicked", SurgListItemView_OnBtnDeleteCacheClicked);
        }
        void SurgListItemView_OnBtnDeleteCacheClicked(object data)
        {
            string bundleName = (string)data;
            _context.ABManager.ClearCache(bundleName);

            OnSpecialtyCategoryItemClicked(null);
        }

        void OnSpecialtyCategoryItemClicked(object data)
        {
            SpecialtyTabView.PresentData presentData = new SpecialtyTabView.PresentData();
            presentData.mode = 1;   // Surgery List.
            presentData.ListSurgeData = new List<SpecialtyTabView.SurgePresentData>();

            mLastSubCategoryKey = data != null ? (string)data : mLastSubCategoryKey;
            presentData.SubCategoryName = mLastSubCategoryKey;
            List<SurgeInfo> surgeInfos = _model.SurgeListModel.GetSurgeInfoFromCategory(mCategoryKey, mLastSubCategoryKey);

            for (int q = 0; q < surgeInfos.Count; ++q)
            {
                SpecialtyTabView.SurgePresentData surgeData = new SpecialtyTabView.SurgePresentData();

                UnityEngine.Assertions.Assert.IsTrue(surgeInfos[q].BundleDependencies != null && surgeInfos[q].BundleDependencies.Count > 0);
                if (surgeInfos[q].BundleDependencies == null || surgeInfos[q].BundleDependencies.Count == 0)
                    continue;

                surgeData.CPTCode = surgeInfos[q].CPTCode;
                surgeData.Name = surgeInfos[q].Name;
                surgeData.Desc = surgeInfos[q].Desc;
                surgeData.IconPath = surgeInfos[q].IconPath;
                surgeData.RVU = surgeInfos[q].RVU;
                surgeData.IsBookmarked = _context.BootStrap.userData.IsBookmarked(surgeInfos[q].CPTCode);

                if (surgeInfos[q].IconPath.Contains("http") || surgeInfos[q].IconPath.Contains("Local"))
                    surgeData.IconPath = surgeInfos[q].IconPath;
                else
                    surgeData.IconPath = $"{_context.BootStrap.setting.CDNProviderHeader}/{surgeInfos[q].IconPath}";


                surgeData.BundleName = surgeInfos[q].BundleDependencies[0].Name;
                surgeData.IsBundleCached = _context.ABManager.IsCached(surgeData.BundleName);
                surgeData.FileSize = _context.ABManager.GetBundleSize(surgeData.BundleName);

                presentData.ListSurgeData.Add(surgeData);
            }
            _view.SpecialtyTabView.Refresh(presentData);
        }

        void OnTabCategoryClicked(object data)
        {
            mCategoryKey = (string)data;

            var categoryInfo = _model.SurgeListModel.GetCategoryInfo(mCategoryKey);
            UnityEngine.Assertions.Assert.IsTrue(categoryInfo != null);
            if (categoryInfo == null)
                return;

            mLastSubCategoryKey = string.Empty;

            SpecialtyTabView.PresentData presentData = new SpecialtyTabView.PresentData();
            presentData.mode = 0;   // Category.

            presentData.SubCategoryName = string.Empty;
            presentData.ListPartCategoryItem = new List<SpecialtyCategoryItemView.PresentData>();

            for (int q = 0; q < categoryInfo.Parts.Count; ++q)
            {
                var listItem = new SpecialtyCategoryItemView.PresentData();
                listItem.CategoryName = categoryInfo.Parts[q].Name;
                listItem.IconPath = categoryInfo.Parts[q].IconPath;

                presentData.ListPartCategoryItem.Add(listItem);
            }

            _view.SpecialtyTabView.Refresh(presentData);
        }

        void OnSpecialtyTabBackBtnClicked(object data)
        {
            OnTabCategoryClicked((object)mCategoryKey);
        }

        //  Private Members ----------------------------------------
        //
        IEnumerator CoInit()
        {
            yield return new WaitUntil(() =>
               _model.SurgeListModel != null &&
               _model.SurgeListModel.CategoryList != null &&
               _model.SurgeListModel.CategoryList.Count > 0);

            OnTabCategoryClicked((object)_model.SurgeListModel.CategoryList[0].Id);
        }
    }
}