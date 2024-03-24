using UnityEngine;
using Core.Events;
using Core.WWW;
using Unity.Services.Authentication;

namespace App.MVCS
{
    public class OptionTabController
    {
        //  Properties ------------------------------------
        //
        SurgeHomeView _view;
        SurgeContext _context;

        EventsGroup Events = new EventsGroup();


        //  Methodss --------------------------------------
        //
        public OptionTabController(SurgeHomeView view, SurgeContext context)
        {
            _view = view;
            _context = context;

            Events.RegisterEvent("OptionTabView_OnClickClearCache", OptionTabView_OnClickClearCache);
            Events.RegisterEvent("SurgeHomeView_OnOptionTabClicked", SurgeHomeView_OnOptionTabClicked);
            Events.RegisterEvent("OptionTabView_OnBtnSignOutClicked", OptionTabView_OnBtnSignOutClicked);
        }


        //  Private Methods ----------------------------------
        //
        void OptionTabView_OnClickClearCache(object data)
        {
            _context.ABManager.ClearCache();
            PlayerPrefs.DeleteAll();
            WWWImageGet.instance.CleanupCache();
            WWWTextGet.instance.CleanupCache();
            _context.BootStrap.userData.Clear();

            _view.OptionTabView.StartView(_context.ABManager.GetCachedSpaceSize());
        }
        void SurgeHomeView_OnOptionTabClicked(object data)
        {
            _view.OptionTabView.StartView(_context.ABManager.GetCachedSpaceSize());
        }
        void OptionTabView_OnBtnSignOutClicked(object data)
        {
            AuthenticationService.Instance.SignOut();
            AuthenticationService.Instance.ClearSessionToken();
        }

    }
}
