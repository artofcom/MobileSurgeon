using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using UnityEngine.UI;
using Core.Tween;

namespace App.MVCS
{
    public class PictureListDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] GameObject scrollView;
        [SerializeField] GameObject picItem;
        [SerializeField] ScrollViewPaginator pageNator;
        [SerializeField] HorizontalLayoutGroup HLG;
        [SerializeField] RectTransform ContentPanel;

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
        List<GameObject> mListObjectItems = new List<GameObject>();
        ScrollRect ScrollRect;

        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


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

        // Methods -   -----------------------------------
        //
        // Start is called before the first frame update
        void Start()
        {
            //var btn = GetComponent<UnityEngine.UI.Button>();
            //btn.onClick.AddListener(this.OnClick);

            Events.RegisterEvent("Zoom2D_OnZoomIn", Zoom2D_OnZoomIn);
            Events.RegisterEvent("Zoom2D_OnZoomOut", Zoom2D_OnZoomOut);
            Events.RegisterEvent("Zoom2D_OnZoomReset", Zoom2D_OnZoomReset);
        }

        // index : note index ( -1 : new one )
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
            ScrollRect = scrollView.GetComponent<ScrollRect>();

            for (int k = 0; k < presentData.ListSprites.Count; ++k)
            {
                GameObject prefab = picItem;
                var obj = GameObject.Instantiate(prefab, ScrollRect.content.transform);
                obj.SetActive(true);
                obj.GetComponent<Image>().sprite = presentData.ListSprites[k];
                mListObjectItems.Add(obj);
            }

            // Init Pos.
            int idxItem = presentData.startIndex;
            float fTargetPos = .0f - (idxItem * (picItem.GetComponent<RectTransform>().rect.width + HLG.spacing));
            ContentPanel.localPosition = new Vector3(fTargetPos, ContentPanel.localPosition.y, ContentPanel.localPosition.z);


            pageNator.Init(presentData.ListSprites.Count);

            // ScrollRect.enabled = false;
        }
        private void OnEnable()
        {
            var introScaler = this.Scaler;
            if (introScaler != null)
                introScaler.Trigger(OnEnableStartScale, OnEnableEndScale, OnEnableDuration);
        }
        private void OnDisable()
        {

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
        void Zoom2D_OnZoomIn(object data)
        {
            ScrollRect.enabled = false;
        }
        void Zoom2D_OnZoomOut(object data)
        {
            ScrollRect.enabled = false;
        }
        void Zoom2D_OnZoomReset(object data)
        {
            ScrollRect.enabled = true;
        }
    }

}