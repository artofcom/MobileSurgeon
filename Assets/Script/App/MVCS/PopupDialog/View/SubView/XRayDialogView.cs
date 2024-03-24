using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Core.Events;
using UnityEngine.UI;
using Core.Tween;

namespace App.MVCS
{
    public class XRayDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] TMP_Text TxtContent;
        //[SerializeField] Button[] XRayButtons;
        [SerializeField] Transform TransformView;
        [SerializeField] Button ButtonResetView;

        [SerializeField] XRayDialogLayoutView[] LayoutViews;

        [Space(10)]
        [Header("---[Intro Animation]---")]
        [SerializeField] Mover Mover;
        [SerializeField] Transform TransformStart, TransformEnd;
        [SerializeField] float TweenDuration = 1.0f;


        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        // Properties ------------------------------------
        //
        System.Action<ReturnData> mCloseCallback = null;
        ReturnData mReturnData = new ReturnData();
        SurgeContext mContext;
        List<Sprite> mListSpritesZoomIn = new List<Sprite>();
        Vector3 mOrgViewPos, mOrgViewScale;


        // Data Model -----------------------------
        //
        public class PresentData : IDialogPresentData
        {
            public SurgeContext Context;
            public string Content;
            public List<Sprite> SpriteBtns = new List<Sprite>();
            public List<Sprite> SpritesZoomIn = new List<Sprite>();
            public List<string> PicNames = new List<string>();
        }
        public class ReturnData : IDialogReturn
        {
            public bool ok { get; set; }
            public void Clear()
            {
                ok = false;
            }
        }


        // Public Methods -   -----------------------------------
        //
        public override void Trigger(IDialogPresentData data, System.Action<IDialogReturn> closeCallBack)
        {
            gameObject.SetActive(true);
            mReturnData.Clear();

            mOrgViewPos = TransformView.localPosition;
            mOrgViewScale = TransformView.localScale;
            ButtonResetView.gameObject.SetActive(false);

            var presentData = data as PresentData;
            if (TxtContent != null && !string.IsNullOrEmpty(presentData.Content))
                TxtContent.text = presentData.Content;

            UnityEngine.Assertions.Assert.IsTrue(LayoutViews != null);
            for (int k = 0; k < LayoutViews.Length; ++k)
            {
                LayoutViews[k].gameObject.SetActive(presentData.SpriteBtns.Count == k + 1);
                if (LayoutViews[k].gameObject.activeSelf)
                    LayoutViews[k].Refresh(presentData.SpriteBtns, presentData.PicNames);
            }

            mContext = presentData.Context;

            mCloseCallback = closeCallBack;
            for (int k = 0; k < presentData.SpritesZoomIn.Count; ++k)
                mListSpritesZoomIn.Add(presentData.SpritesZoomIn[k]);
        }




        private void OnEnable()
        {
            var introScaler = this.Mover;
            if (introScaler != null)
                introScaler.Trigger(TransformStart.localPosition, TransformEnd.localPosition, TweenDuration);
        }



        // Event Handlers  -----------------------------------
        //
        public void OnClose()
        {
            gameObject.SetActive(false);

            mReturnData.ok = false;
            mListSpritesZoomIn.Clear();
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }

        public void OnClickButtonReset()
        {
            TransformView.localPosition = mOrgViewPos;
            TransformView.localScale = mOrgViewScale;
            ButtonResetView.gameObject.SetActive(false);
        }
        public void OnClickXRayButton(int idx)
        {
            TriggerXRayImageZoomInDialog(idx);
        }


        // Private Methods -   -----------------------------------
        //
        void TriggerXRayImageZoomInDialog(int idx)
        {
            //var presendData = new XRayZoomInDialogView.PresentData();
            //presendData.sprite = mListSpritesZoomIn[idx];
            //mContext.TriggerDialog("XRayZoomInDialog", presendData, (returnedData) => { });

            if (mListSpritesZoomIn.Count <= idx)
                return;

            //var presentData = new PageSliderDialogView.PresentData();
            var presentData = new XRayPageViewDialog.PresentData();
            for (int k = 0; k < mListSpritesZoomIn.Count; ++k)
                presentData.ListSprites.Add(mListSpritesZoomIn[k]);

            presentData.startIndex = idx;
            //mContext.TriggerDialog("picListDialog", presentData, (returnedData) => { });
            mContext.TriggerDialog("XRayPageViewDialog", presentData, (returnedData) => { });

            // Reset Transform.
            OnClickButtonReset();
        }

    }
}

//var presentData = new PictureListDialogView.PresentData();
//for (int k = 0; k < mListSpritesZoomIn.Count; ++k)
//    presentData.ListSprites.Add(mListSpritesZoomIn[k]);

//presentData.startIndex = idx;
//mContext.TriggerDialog("picListDialog", presentData, (returnedData) =>{ });
