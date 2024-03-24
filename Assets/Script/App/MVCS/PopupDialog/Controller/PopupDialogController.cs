using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using UnityEngine.Assertions;
using System;

namespace App.MVCS
{
    public class PopupDialogController : IController
    {
        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();

        //  Properties ------------------------------------
        PopupDialogModel _model;
        PopupDialogView _view;
        PopupDialogService _service;
        SurgeContext _context;

        DialogView mActivatedDialog;

        //  Methods ---------------------------------------
        public PopupDialogController(PopupDialogModel model, PopupDialogView view, PopupDialogService service, SurgeContext context)
        {
            _model = model;
            _view = view;
            _service = service;
            _context = context;

            //Events.RegisterEvent("PhaseItemView_OnSurgSectionItemClicked", PhaseItemView_OnSurgSectionItemClicked);
            //Events.RegisterEvent("PhaseItemView_OnBtnDeleteCacheClicked", PhaseItemView_OnBtnDeleteCacheClicked);
        }


        public void TriggerDialog(string strDialogName, IDialogPresentData presentData, Action<IDialogReturn> callbackDone)
        {
            DialogView dlgView = _view.GetDialogView(strDialogName);
            if (dlgView == null)
            {
                callbackDone(null);
                return;
            }
            dlgView.dialogView.Trigger(presentData, callbackDone);

            mActivatedDialog = dlgView;
        }
        public void CloseDialog(string strDlgName)
        {
            if (mActivatedDialog == null)
                return;

            if (string.IsNullOrEmpty(strDlgName) || (!string.IsNullOrEmpty(strDlgName) && mActivatedDialog.name.ToLower() == strDlgName.ToLower()))
            {
                mActivatedDialog.dialogView.gameObject.SetActive(false);
                mActivatedDialog = null;
            }
        }



        // Event Handler. ---------------------------------
        //
        //void SurgeHomeView_OnOptionTabClicked(object data)
        //{
        //    _view.StartV .StartCoroutine(coSurgeSummaryView_OnEnable());
        //}






        // Private Method. ---------------------------------
        //

    }
}
