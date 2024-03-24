using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.MVCS
{
    public class SurgeAnimation : IMVCS
    {
        //  Events ----------------------------------------

        //  Properties ------------------------------------
        SurgeAnimationModel _model;
        SurgeAnimationView _view;
        SurgeAnimationController _controller;
        SurgeAnimationService _service;
        SurgeContext _context;
        bool _isInitialized = false;

        //  Fields ----------------------------------------

        //  Methods ---------------------------------------
        public SurgeAnimation(SurgeAnimationView view, SurgeContext context)
        {
            _view = view;
            _context = context;
        }
        public void Initialize()
        {
            if (_isInitialized) return;

            _model = new SurgeAnimationModel();
            _service = new SurgeAnimationService(_model, _context);
            _controller = new SurgeAnimationController(_model, _view, _service, _context);


            //
            // TEMP Code, since we know we not gonna have the Summary View.
            //
            //_context.SurgeAniService = _service;
        }
    }
}
