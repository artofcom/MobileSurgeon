using UnityEngine;
using TMPro;
using Core.Events;

namespace App.MVCS
{
    public class BundleDownloadDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] RectTransform ProgressBar;
        [SerializeField] TMP_Text txtDesc;


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
            public string BundleName;
        }
        public class ReturnData : IDialogReturn
        {
            public bool ok { get; set; }
            public void Clear()
            {
                ok = false;
            }
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

            Events.RegisterEvent("OnAssetBundleDownloadProgress", OnAssetBundleDownloadProgress);

            mReturnData.Clear();
            mCloseCallback = closeCallBack;
            if (txtDesc != null)
                txtDesc.text = $"Downloading {presentData.BundleName}...";
        }




        // Event Handlers  -----------------------------------
        //
        public void OnBtnXClicked()
        {
            gameObject.SetActive(false);

            mReturnData.ok = false;
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }
        void OnAssetBundleDownloadProgress(object data)
        {
            float prog = (float)data;
            ProgressBar.transform.localScale = new Vector3(prog, 1.0f, 1.0f);

            if (prog >= 1.0f)
            {
                gameObject.SetActive(false);

                mReturnData.ok = true;
                if (mCloseCallback != null)
                    mCloseCallback.Invoke(mReturnData);
            }
        }
    }

}