using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.MVCS
{
    public class SurgeHome : IMVCS
    {
        //  Events ----------------------------------------

        //  Properties ------------------------------------
        SurgeHomeModel _homeModel;
        SurgeHomeView _view;
        SurgeHomeController _controller;
        SurgeHomeService _service;
        SurgeContext _context;
        bool _isInitialized = false;


        //  Methods ---------------------------------------
        public SurgeHome(SurgeHomeView view, SurgeContext context)
        {
            _view = view;
            _context = context;
        }
        public void Initialize()
        {
            if (_isInitialized) return;

            _homeModel = new SurgeHomeModel();
            _service = new SurgeHomeService(_homeModel, _context);
            _controller = new SurgeHomeController(_homeModel, _view, _service, _context);
        }
    }
}
