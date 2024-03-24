using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

namespace App.MVCS
{
    public class CPTTabController
    {
        //  Events ----------------------------------------
        //
        EventsGroup Events = new EventsGroup();

        public enum eSORT_TYPE
        {
            CPT, RVU, NAME,
        };


        //  Property --------------------------------------
        //
        SurgeHomeView _view;
        SurgeHomeModel _model;
        SurgeContext _context;

        eSORT_TYPE mSortType = eSORT_TYPE.CPT;
        bool mAscendingSort = true;


        //  Methods ----------------------------------------
        //
        public CPTTabController(SurgeHomeView view, SurgeHomeModel model, SurgeContext context)
        {
            _view = view;
            _model = model;
            _context = context;


            Events.RegisterEvent("CPTTabView_OnEnabled", CPTTabView_OnEnabled);
            Events.RegisterEvent("CPTTabView_OnDisabled", CPTTabView_OnDisabled);
        }


        void CPTTabView_OnEnabled(object data)
        {
            Events.RegisterEvent("CPTTabView_OnSearchFieldChanged", CPTTabView_OnSearchFieldChanged);
            Events.RegisterEvent("SurgListItemView_OnBtnDeleteCacheClicked", SurgListItemView_OnBtnDeleteCacheClicked);

            Events.RegisterEvent("CPTTabView_OnCPTSortClicked", CPTTabView_OnCPTSortClicked);
            Events.RegisterEvent("CPTTabView_OnRVUSortClicked", CPTTabView_OnRVUSortClicked);
            Events.RegisterEvent("CPTTabView_OnNameSortClicked", CPTTabView_OnNameSortClicked);

            _view.StartCoroutine(coCPTTabView_refresh());
        }
        void CPTTabView_OnDisabled(object data)
        {
            Events.UnRegisterEvent("CPTTabView_OnSearchFieldChanged", CPTTabView_OnSearchFieldChanged);
            Events.UnRegisterEvent("SurgListItemView_OnBtnDeleteCacheClicked", SurgListItemView_OnBtnDeleteCacheClicked);

            Events.UnRegisterEvent("CPTTabView_OnCPTSortClicked", CPTTabView_OnCPTSortClicked);
            Events.UnRegisterEvent("CPTTabView_OnRVUSortClicked", CPTTabView_OnRVUSortClicked);
            Events.UnRegisterEvent("CPTTabView_OnNameSortClicked", CPTTabView_OnNameSortClicked);
        }
        void CPTTabView_OnSearchFieldChanged(object data)
        {
            _view.StartCoroutine(coCPTTabView_refresh());
        }
        void SurgListItemView_OnBtnDeleteCacheClicked(object data)
        {
            string bundleName = (string)data;
            _context.ABManager.ClearCache(bundleName);

            _view.StartCoroutine(coCPTTabView_refresh());
        }
        void CPTTabView_OnCPTSortClicked(object data)
        {
            mSortType = eSORT_TYPE.CPT;
            mAscendingSort = !mAscendingSort;
            _view.StartCoroutine(coCPTTabView_refresh());
        }
        void CPTTabView_OnRVUSortClicked(object data)
        {
            mSortType = eSORT_TYPE.RVU;
            mAscendingSort = !mAscendingSort;
            _view.StartCoroutine(coCPTTabView_refresh());
        }
        void CPTTabView_OnNameSortClicked(object data)
        {
            mSortType = eSORT_TYPE.NAME;
            mAscendingSort = !mAscendingSort;
            _view.StartCoroutine(coCPTTabView_refresh());
        }

        //  Private Methodes ----------------------------------------
        //
        IEnumerator coCPTTabView_refresh()
        {
            while (_model.SurgeListModel == null)
                yield return null;

            var surgeListInfo = _model.SurgeListModel;
            CPTTabView.PresentData presentData = new CPTTabView.PresentData();
            presentData.ItemPresentData = new List<SurgListItemView.PresentData>();

            presentData.SortType = mSortType;
            presentData.bAscendingSort = mAscendingSort;

            for (int k = 0; k < surgeListInfo.SurgeryList.Count; ++k)
            {
                var data = surgeListInfo.SurgeryList[k];

                UnityEngine.Assertions.Assert.IsTrue(data.BundleDependencies != null && data.BundleDependencies.Count > 0);
                if (data.BundleDependencies == null || data.BundleDependencies.Count == 0)
                    continue;


                SurgListItemView.PresentData itemPresentData = new SurgListItemView.PresentData();
                itemPresentData.CPTCode = data.CPTCode;
                itemPresentData.Desc = data.Desc;
                itemPresentData.Name = data.Name;
                itemPresentData.RVU = data.RVU;
                itemPresentData.IsBookmarked = _context.BootStrap.userData.IsBookmarked(data.CPTCode);

                itemPresentData.BundleName = data.BundleDependencies[0].Name;
                itemPresentData.FileSize = _context.ABManager.GetBundleSize(data.BundleDependencies[0].Name);
                itemPresentData.IsBundleCached = _context.ABManager.IsCached(data.BundleDependencies[0].Name);

                if (data.IconPath.Contains("http") || data.IconPath.Contains("Local"))
                    itemPresentData.IconPath = data.IconPath;
                else
                    itemPresentData.IconPath = $"{_context.BootStrap.setting.CDNProviderHeader}/{data.IconPath}";

                presentData.ItemPresentData.Add(itemPresentData);
            }

            _view.CPTTabView.Refresh(presentData);
        }
    }
}
