using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Tween;

namespace App.MVCS
{
    public class XRayZoomInDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] Image image;


        [Space(10)]
        [Header("---[Intro Animation]---")]
        [SerializeField] Scaler Scaler;
        [SerializeField] float OnEnableStartScale = .0f;
        [SerializeField] float OnEnableEndScale = 1.0f;
        [SerializeField] float OnEnableDuration = 1.0f;


        // Properties ------------------------------------
        //
        System.Action<ReturnData> mCloseCallback = null;
        ReturnData mReturnData = new ReturnData();


        // Data Model -----------------------------
        //
        public class PresentData : IDialogPresentData
        {
            // public string Content;
            public int startIndex = 0;
            public Sprite sprite = null;
        }
        public class ReturnData : IDialogReturn
        {
            public bool ok { get; set; }
            public void Clear()
            {
                ok = false;
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Trigger(IDialogPresentData data, System.Action<IDialogReturn> closeCallBack)
        {
            gameObject.SetActive(true);
            mCloseCallback = closeCallBack;

            PresentData presentData = data as PresentData;
            image.sprite = presentData.sprite;
        }
        private void OnEnable()
        {
            var introScaler = this.Scaler;
            if (introScaler != null)
                introScaler.Trigger(OnEnableStartScale, OnEnableEndScale, OnEnableDuration);
        }


        // Event Handlers  -----------------------------------
        //
        public void OnClose()
        {
            gameObject.SetActive(false);

            mReturnData.ok = false;
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }
    }
}
