using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
{
    public class PopupDialog : IMVCS
    {
        //  Events ----------------------------------------

        //  Properties ------------------------------------
        PopupDialogModel _model;
        PopupDialogView _view;
        PopupDialogController _controller;
        PopupDialogService _service;
        SurgeContext _context;
        bool _isInitialized = false;

        //  Fields ----------------------------------------

        //  Methods ---------------------------------------
        public PopupDialog(PopupDialogView view, SurgeContext context)
        {
            _view = view;
            _context = context;
        }
        public void Initialize()
        {
            if (_isInitialized) return;

            _model = new PopupDialogModel();
            _service = new PopupDialogService(_context, _model);
            _controller = new PopupDialogController(_model, _view, _service, _context);
        }

        public void TriggerDialog(string strDialogName, IDialogPresentData data, Action<IDialogReturn> callbackDone)
        {
            _controller.TriggerDialog(strDialogName, data, callbackDone);
        }
        public void CloseDialog(string strName)
        {
            _controller.CloseDialog(strName);
        }
    }
}
