using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
{
    public class SurgeLoadingService : IService
    {
        SurgeLoadingModel _model;
        SurgeContext _context;

        public SurgeLoadingService(SurgeLoadingModel model, SurgeContext context)
        {
            _model = model;
            _context = context;
        }

    }
}