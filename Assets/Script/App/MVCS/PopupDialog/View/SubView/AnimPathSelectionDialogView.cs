using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core.Events;

namespace App.MVCS
{
    public class AnimPathSelectionDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] GameObject Group2Options;
        [SerializeField] GameObject Group3Options;


        // Properties ------------------------------------
        //
        System.Action<ReturnData> mCloseCallback = null;
        ReturnData mReturnData = new ReturnData();

        GameObject mSelectedOptionGroup = null;

        // Models -----------------------------
        //
        public class PresentData : IDialogPresentData
        {
            public List<AnimPathSubView.PathInfo> PathOptions = new List<AnimPathSubView.PathInfo>();
        }
        public class ReturnData : IDialogReturn
        {
            public int IndexSelected { get; set; }
            public void Clear()
            { }
        }



        // Methods -   -----------------------------------
        //
        // Start is called before the first frame update
        void Start() { }


        // index : note index ( -1 : new one )
        public override void Trigger(IDialogPresentData data, System.Action<IDialogReturn> closeCallBack)
        {
            var presentData = data as PresentData;
            gameObject.SetActive(true);

            int optionCnt = presentData.PathOptions.Count;
            Group2Options.SetActive(false);
            Group3Options.SetActive(false);

            switch (optionCnt)
            {
                case 2: mSelectedOptionGroup = Group2Options; break;
                case 3: mSelectedOptionGroup = Group3Options; break;
                default: break;
            }

            if (mSelectedOptionGroup != null)
            {
                mSelectedOptionGroup.SetActive(true);
                mSelectedOptionGroup.GetComponent<AnimPathSubView>().Refresh(presentData.PathOptions);
            }

            mReturnData.Clear();
            mCloseCallback = closeCallBack;
            //if (txtMessage != null)
            //    txtMessage.text = presentData.Message;
        }




        // Event Handlers  -----------------------------------
        //
        public void OnBtnOkClicked()
        {
            mReturnData.IndexSelected = mSelectedOptionGroup.GetComponent<AnimPathSubView>().SelectedIndex;

            gameObject.SetActive(false);
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }
    }
}
