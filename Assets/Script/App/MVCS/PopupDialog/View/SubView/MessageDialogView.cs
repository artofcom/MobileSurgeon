using UnityEngine;
using TMPro;
using Core.Events;

namespace App.MVCS
{
    public class MessageDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] TMP_Text txtMessage;


        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        // Properties ------------------------------------
        //
        System.Action<ReturnData> mCloseCallback = null;
        ReturnData mReturnData = new ReturnData();



        // Models -----------------------------
        //
        public class PresentData : IDialogPresentData
        {
            public string Message;
        }
        public class ReturnData : IDialogReturn
        {
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

            mReturnData.Clear();
            mCloseCallback = closeCallBack;
            if (txtMessage != null)
                txtMessage.text = presentData.Message;
        }




        // Event Handlers  -----------------------------------
        //
        public void OnBtnOkClicked()
        {
            gameObject.SetActive(false);
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }
    }

}