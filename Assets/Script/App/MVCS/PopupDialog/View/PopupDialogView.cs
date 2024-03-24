using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Core.Events;
using TMPro;
using System;

namespace App.MVCS
    { 
    public interface IDialogReturn
    { }
    public interface IDialogPresentData
    { }

    public abstract class PopupDialogScreen : AView
    {
        public abstract void Trigger(IDialogPresentData presentData, Action<IDialogReturn> callbackDone);
    }


    [System.Serializable]
    public class DialogView
    {
        public string name;
        public PopupDialogScreen dialogView;
    }

    public class PopupDialogView : AView
    {
        // Consts ------------------------------------
        //
        // const string FALLBACK_PATH = "UX/Icon/fallback";


        // Serialized Field -------------------------------------
        //
        [SerializeField] DialogView[] DialogViews;


        //  Properties ------------------------------------
        //
        //List<GameObject> mListItems = new List<GameObject>();


        // Presentation Model -----------------------------
        //
        public class PresentData
        {
            // public string IconPath;
            // public List<PhaseItemView.PresentData> listItemData;
            // public bool IsExpertMode;
        }



        // Public Methods. ---------------------------------
        //
        public DialogView GetDialogView(string strDialogName)
        {
            if (DialogViews == null || DialogViews.Length == 0)
                return null;

            for (int k = 0; k < DialogViews.Length; ++k)
            {
                if (DialogViews[k].name == strDialogName)
                    return DialogViews[k];
            }
            return null;
        }


        // Event Handler. ---------------------------------
        //

    }

}
