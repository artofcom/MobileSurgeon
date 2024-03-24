using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace App.MVCS
{
    public class PopupDialogService : IService
    {
        // [Members] -----------------------------------
        //
        PopupDialogModel _model;
        SurgeContext _context;



        // [Public Members] -----------------------------
        //
        public PopupDialogService(SurgeContext context, PopupDialogModel model)
        {
            _context = context;
            _model = model;
        }
    }
}


#region == COMMENTS


/*
   public IEnumerator coLoadSurgeryDetailInfo(string dataPath)
   {
       _model.SurgeSummaryInfo = null;

       if (dataPath.ToLower().Contains("http"))
       {
           bool fetching = true;
           WWWTextGet.GetDataForTextURLExt(dataPath, (string loadedText, string textUrl) =>
           {
               _model.SurgeSummaryInfo = JsonUtility.FromJson<SurgeDetailInfo>(loadedText);
               fetching = false;
               Debug.Log(textUrl + " Downloaded successfully.");
           },
           (string textUrl, string error) =>
           {
               Debug.Log(textUrl + " Downloading has been failed... " + error);
               fetching = false;
           },
           useCache: false);

           while (fetching)
               yield return null;

           if (_model.SurgeSummaryInfo == null)
           {
               fetching = true;
               WWWTextGet.GetDataForTextURLExt(dataPath, (string loadedText, string textUrl) =>
               {
                   _model.SurgeSummaryInfo = JsonUtility.FromJson<SurgeDetailInfo>(loadedText);
                   fetching = false;
                   Debug.Log(textUrl + " Downloaded successfully.");
               },
               (string textUrl, string error) =>
               {
                   Debug.Log(textUrl + " Downloading has been failed... " + error);
                   fetching = false;
               },
               useCache: true);

               while (fetching)
                   yield return null;
           }
       }
       else
       {
           string strJson = Resources.Load<TextAsset>(dataPath).text;
           _model.SurgeSummaryInfo = JsonUtility.FromJson<SurgeDetailInfo>(strJson);
       }

       // For this one, there's alwyas a chance failing to load the data file.
       if (_model.SurgeSummaryInfo == null)
       {
           Debug.LogWarning("Warning) Have failed to load SurgeSummary Info.. " + dataPath);
       }
   }*/

#endregion