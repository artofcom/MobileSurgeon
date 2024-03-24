using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using UnityEngine.Assertions;
using App.Data;
using Core.WWW;

namespace App.MVCS
{
    public class HomeTabController
    {
        //  Properties ------------------------------------
        //
        SurgeHomeView _view;
        SurgeHomeService _service;
        SurgeContext _context;

        EventsGroup Events = new EventsGroup();


        //  Methodss --------------------------------------
        //
        public HomeTabController(SurgeHomeView view, SurgeHomeService service, SurgeContext context)
        {
            _view = view;
            _service = service;
            _context = context;

            Events.RegisterEvent("HomeTabView_OnEnabled", HomeTabView_OnEnabled);
            Events.RegisterEvent("OnRequestRefreshView", OnRequestRefreshView);
        }


        //  Event Handlers ----------------------------------
        //
        void HomeTabView_OnEnabled(object data)
        {
            Debug.Log("Home Tab View Enabled.");

            _view.StartCoroutine(coInitView());
        }
        void OnRequestRefreshView(object data)
        {
            HomeTabView_OnEnabled(data);
        }






        //  Private Methods ----------------------------------
        //
        IEnumerator coInitView()
        {
            yield return _view.StartCoroutine(coInitConfigs());

            refreshUI();
        }


        void refreshUI()
        {
            var homeCardListInfo = _service.SurgeHomeModel.HomeViewModel;
            Assert.IsTrue(homeCardListInfo != null);

            // Internal Func.
            //
            HomeItemDefine.PresentModel MakeSectionItem(string tagName)
            {
                HomeItemDefine.PresentModel itemSection = new HomeItemDefine.PresentModel();
                itemSection.Type = "SECTION";
                itemSection.Tag = tagName;
                return itemSection;
            }
            HomeItemDefine.PresentModel MakeHomeViewItem(int CPTCode, string ItemType, string Tag, string Desc = "", string IconPath = "", bool canRemoveFromBookmark = false)
            {
                SurgeInfo surgeInfo = _service.SurgeHomeModel.GetSurgeInfo(CPTCode);
                if (surgeInfo == null)
                    return null;

                HomeItemDefine.PresentModel model = new HomeItemDefine.PresentModel();
                model.CPTCode = CPTCode;
                model.Type = ItemType;
                model.Desc = string.IsNullOrEmpty(Desc) ? surgeInfo.Desc : Desc;
                model.Tag = Tag;
                model.RVU = surgeInfo.RVU;
                model.CanRemoveFromBookmark = canRemoveFromBookmark;
                string iconPath = !string.IsNullOrEmpty(IconPath) ? IconPath : (ItemType == "BIG" ? surgeInfo.BigIconPath : surgeInfo.IconPath);
                if (iconPath.Contains("http") || iconPath.Contains("Local"))
                    model.ImagePath = iconPath;
                else
                    model.ImagePath = $"{_context.BootStrap.setting.CDNProviderHeader}/{iconPath}";

                return model;
            }

            // Build Presentation Data.
            //
            bool HasRecentLearning = _context.BootStrap.userData.LastRecentLearningId > 0;
            HomeTabView.PresentData homeTabData = new HomeTabView.PresentData();
            homeTabData.listItemInfo = new List<HomeItemDefine.PresentModel>();
            if (HasRecentLearning)
            {
                homeTabData.RecentLearning = MakeHomeViewItem(_context.BootStrap.userData.LastRecentLearningId, "BIG", "Continue Learning..");

                if (homeTabData.RecentLearning != null && homeCardListInfo.CardList.Count > 0)
                    homeTabData.listItemInfo.Add(MakeSectionItem("Featured."));
                if (homeTabData.RecentLearning == null)
                    HasRecentLearning = false;
            }

            for (int k = 0; k < homeCardListInfo.CardList.Count; ++k)
            {
                HomeCardInfo info = homeCardListInfo.CardList[k];

                SurgeInfo surgeInfo = _service.SurgeHomeModel.GetSurgeInfo(info.CPTCode);
                Assert.IsTrue(surgeInfo != null);
                if (surgeInfo == null) continue;

                bool makeData = false;
                if (k == 0)
                {
                    if (!HasRecentLearning || info.Type != "BIG")
                        makeData = true;
                }
                else makeData = true;


                if (makeData)
                {
                    HomeItemDefine.PresentModel itemData = MakeHomeViewItem(info.CPTCode, info.Type, info.Tag, "", info.IconPath);
                    if (itemData == null)
                        continue;

                    homeTabData.listItemInfo.Add(itemData);

                    if (k == 0 && itemData.Type == "BIG" && homeCardListInfo.CardList.Count > 1)
                        homeTabData.listItemInfo.Add(MakeSectionItem("Featured."));
                }
            }

            if (_context.BootStrap.userData.BookMarkList.Count > 0)
                homeTabData.listItemInfo.Add(MakeSectionItem("Bookmarked."));

            for (int k = 0; k < _context.BootStrap.userData.BookMarkList.Count; ++k)
            {
                SurgeInfo surgeInfo = _service.SurgeHomeModel.GetSurgeInfo(_context.BootStrap.userData.BookMarkList[k]);
                Assert.IsTrue(surgeInfo != null);
                if (surgeInfo == null) continue;

                HomeItemDefine.PresentModel itemData = MakeHomeViewItem(surgeInfo.CPTCode, "SMALL", "", surgeInfo.Desc, "", canRemoveFromBookmark: true);
                if (itemData == null)
                    continue;

                homeTabData.listItemInfo.Add(itemData);
            }

            _view.HomeTabView.Refresh(homeTabData);
        }



        IEnumerator coInitConfigs()
        {
            WWWTextGet.SetOfflineMode(!_context.BootStrap.setting.UseRemoteConfig);
            WWWImageGet.SetOfflineMode(!_context.BootStrap.setting.UseRemoteConfig);

            // Load all configs files.
            _service.Init(_view.HomeTabView, () =>
            {
                if (_service.SurgeHomeModel.DirectorModel != null && _service.SurgeHomeModel.DirectorModel.DebugInfo != null)
                {
                    string tag = _service.SurgeHomeModel.DirectorModel.DebugInfo.tag == null ? "no_tag" : _service.SurgeHomeModel.DirectorModel.DebugInfo.tag;
                    Debug.Log("> Director info tag : " + tag);
                }
            });

            // Make sure to wait until it's done.
            while (_service.SurgeHomeModel.HomeViewModel == null || _service.SurgeHomeModel.SurgeListModel == null)
                yield return null;


            // Cache Surgery list to use golbally.
            //_context.SurgeryListRef = _service.SurgeHomeModel.SurgeListModel.SurgeryList;


            // Load surgery info files. - Don't have to do this anymore.
            /*var userData = BootStrap.GetInstance().userData;
            if (!string.IsNullOrEmpty(userData.LastRecentLearningId))
            {
                SurgeInfo info = _service.SurgeHomeModel.SurgeListModel.GetSurgeryInfo(userData.LastRecentLearningId);
                yield return _view.StartCoroutine(context.BootStrap.coLoadSurgeryDetailInfo(info.DataPath));
            }*/
        }
    }

}