using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TS.PageSlider;
using Core.Tween;
using Core.UI;

namespace App.MVCS
{
    public class XRayPageViewDialog : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] PinchablePageScroller Slider;
        [SerializeField] PageDotsIndicator DotsIndicator;
        [SerializeField] GameObject Item;

        [Space(10)]
        [Header("---[Intro Animation]---")]
        [SerializeField] Scaler Scaler;
        [SerializeField] float OnEnableStartScale = .0f;
        [SerializeField] float OnEnableEndScale = 1.0f;
        [SerializeField] float OnEnableDuration = 1.0f;


        // Data Model -----------------------------
        //
        public class PresentData : IDialogPresentData
        {
            // public string Content;
            public int startIndex = 0;
            public List<Sprite> ListSprites = new List<Sprite>();
        }
        public class ReturnData : IDialogReturn
        {
            public bool ok { get; set; }
            public void Clear()
            {
                ok = false;
            }
        }


        // Properties ------------------------------------
        //
        System.Action<ReturnData> mCloseCallback = null;
        ReturnData mReturnData = new ReturnData();
        List<GameObject> mListObjectItems = new List<GameObject>();


        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            var introScaler = this.Scaler;
            if (introScaler != null)
                introScaler.Trigger(OnEnableStartScale, OnEnableEndScale, OnEnableDuration);
        }

        public override void Trigger(IDialogPresentData data, System.Action<IDialogReturn> closeCallBack)
        {
            gameObject.SetActive(true);
            mCloseCallback = closeCallBack;

            PresentData presentData = data as PresentData;
            // ScrollRect = scrollView.GetComponent<ScrollRect>();

            for (int k = 0; k < presentData.ListSprites.Count; ++k)
            {
                var obj = GameObject.Instantiate(Item);//, ScrollRect.content.transform);
                obj.SetActive(true);
                obj.GetComponent<Image>().sprite = presentData.ListSprites[k];
                mListObjectItems.Add(obj);
                DotsIndicator.Add();

                Slider.AddTargetView((RectTransform)obj.transform);
            }
            DotsIndicator.IsVisible = mListObjectItems.Count > 0;

            Slider.OnPageChangeEnded.AddListener(OnPageChangeEnded);
            Slider.Trigger(presentData.startIndex);
        }

        // Event Handlers  -----------------------------------
        //
        void OnPageChangeEnded(int curPageIndex)
        {
            DotsIndicator?.SetActiveDot(curPageIndex);
        }
        public void OnClose()
        {
            mReturnData.ok = false;
            // mListSpritesZoomIn.Clear();
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);


            Slider.OnPageChangeEnded.RemoveListener(OnPageChangeEnded);
            Slider.Clear();
            DotsIndicator.Clear();

            if (mListObjectItems.Count > 0)
            {
                for (int k = 0; k < mListObjectItems.Count; ++k)
                    GameObject.Destroy(mListObjectItems[k]);
                mListObjectItems.Clear();
            }
            gameObject.SetActive(false);
        }
    }
}
