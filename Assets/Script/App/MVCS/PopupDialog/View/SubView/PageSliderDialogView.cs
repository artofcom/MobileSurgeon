using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TS.PageSlider;
using UnityEngine.UI;
using Core.Events;
using Core.Tween;

namespace App.MVCS
{
    public class PageSliderDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] PageSlider Slider;
        [SerializeField] GameObject ScrollItem;
        [SerializeField] RectTransform ContentPanel;
        [SerializeField] ScrollRect ScrollRect;

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


        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();

        // Start is called before the first frame update
        void Start()
        {
            Events.RegisterEvent("Zoom2D_OnZoomUpdated", Zoom2D_OnZoomUpdated);
            Events.RegisterEvent("Zoom2D_OnZoomReset", Zoom2D_OnZoomReset);
        }

        // Update is called once per frame
        void Update()
        {
            // ScrollRect.enabled = Input.touchCount == 2;
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

            if (mListObjectItems.Count > 0)
            {
                for (int k = 0; k < mListObjectItems.Count; ++k)
                    GameObject.Destroy(mListObjectItems[k]);
                mListObjectItems.Clear();
            }

            PresentData presentData = data as PresentData;
            // ScrollRect = scrollView.GetComponent<ScrollRect>();

            for (int k = 0; k < presentData.ListSprites.Count; ++k)
            {
                var obj = GameObject.Instantiate(ScrollItem);//, ScrollRect.content.transform);
                obj.SetActive(true);
                obj.GetComponent<Image>().sprite = presentData.ListSprites[k];
                mListObjectItems.Add(obj);

                Slider.AddPage((RectTransform)obj.transform);
            }


            // Init Pos.
            int idxItem = presentData.startIndex;
            StartCoroutine(coSetScrollPos(idxItem));

            // Test.
            // ScrollRect.enabled = false;
        }

        IEnumerator coSetScrollPos(int idxItem)
        {
            yield return new WaitForSeconds(0.05f); // Give some time.
            Slider.ForceInitPage(idxItem);
        }

        // Event Handlers  -----------------------------------
        //
        public void OnClose()
        {
            gameObject.SetActive(false);

            mReturnData.ok = false;
            // mListSpritesZoomIn.Clear();
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
            Slider.Clear();
        }
        void Zoom2D_OnZoomUpdated(object data)
        {
            ScrollRect.enabled = false;
        }
        void Zoom2D_OnZoomReset(object data)
        {
            ScrollRect.enabled = true;
        }
    }
}
