using UnityEngine;
using TMPro;
using Core.Events;
using Core.WWW;

namespace App.MVCS
{
    public class SurgListItemView : AView
    {
        //  Constants -----------------------------------------
        //
        const string FALLBACK_PATH = "UX/Icon/fallback";


        //  Properties ----------------------------------------
        //
        [SerializeField] TMP_Text txtTitle;
        [SerializeField] TMP_Text txtCPT, txtRVU;
        [SerializeField] UnityEngine.UI.Image ImgIcon;
        [SerializeField] GameObject ImageLoading;
        [SerializeField] GameObject BtnDownloadItem;
        [SerializeField] GameObject BtnDeleteCache;
        [SerializeField] TMP_Text txtDownloadInfo;
        [SerializeField] GameObject BtnAddToBookMark;
        [SerializeField] GameObject BtnRemoveFromBookMark;

        int mCPTCode;
        string mBundleName;

        //  Presentation Model ---------------------------------
        //
        public class PresentData
        {
            public int CPTCode;
            public string Name, IconPath, Desc;
            public float RVU;
            public string BundleName;
            public bool IsBundleCached, IsBookmarked;
            public long FileSize;
            // public string CategoryId, SubCategoryId;
        }



        //  Mono Event Hanlders ----------------------------------
        //
        private void Awake()
        {
            ImageLoading.SetActive(false);
            BtnDeleteCache.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            var btn = GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(this.OnClick);
        }




        //  Methods ----------------------------------------
        //
        public void Refresh(PresentData info)
        {
            string bundleStatus = info.IsBundleCached ? "Cached." : $"Download Req - [{ToFileSize(info.FileSize)}]";

            //txtTitle.text = $"CPT {info.CPTCode} : RVU {info.RVU}";
            //txtDesc.text = $"{info.Desc} : {bundleStatus}";
            txtTitle.text = info.Name;
            txtCPT.text = $"{info.CPTCode}";
            txtRVU.text = $"{info.RVU}";

            BtnAddToBookMark.SetActive(!info.IsBookmarked);
            BtnRemoveFromBookMark.SetActive(info.IsBookmarked);

            BtnDownloadItem.SetActive(!info.IsBundleCached);
            BtnDeleteCache.SetActive(info.IsBundleCached);
            if (!info.IsBundleCached)
                txtDownloadInfo.text = $"Download \n{ToFileSize(info.FileSize)}";

            mCPTCode = info.CPTCode;
            mBundleName = info.BundleName;

            string picURL = info.IconPath;
            if (picURL.ToLower().Contains("http"))
            {
                ImageLoading.SetActive(true);
                WWWImageGet.GetDataForImageURL(picURL, (Texture2D loadedTexture, string imageUrl) =>
                {
                    ImgIcon.sprite = CreateSprite(loadedTexture, "AAA");
                    ImageLoading.SetActive(false);
                    Debug.Log(imageUrl + " Downloaded successfully.");
                },
                (string imageUrl, string error) =>
                {
                    Debug.Log(imageUrl + " Downloading has been failed... " + error);
                    var sprite = Resources.Load<Sprite>(FALLBACK_PATH);
                    ImgIcon.sprite = sprite;
                    ImageLoading.SetActive(false);
                });
            }
            else
            {
                var sprite = Resources.Load<Sprite>(picURL);
                if (sprite == null)
                    sprite = Resources.Load<Sprite>(FALLBACK_PATH);
                ImgIcon.sprite = sprite;
                ImageLoading.SetActive(false);
            }
        }


        //  Event Handlers ----------------------------------------
        //
        public void OnClick()
        {
            EventSystem.DispatchEvent("OnSurgItemClicked", (object)mCPTCode);
        }
        public void OnBtnDeleteCacheClicked()
        {
            EventSystem.DispatchEvent("SurgListItemView_OnBtnDeleteCacheClicked", (object)mBundleName);
        }
        public void OnAddToBookMark()
        {
            EventSystem.DispatchEvent("OnAddToBookMark", (object)mCPTCode);
            BtnAddToBookMark.SetActive(false);
            BtnRemoveFromBookMark.SetActive(true);
        }
        public void OnRemoveFromBookMark()
        {
            EventSystem.DispatchEvent("OnRemoveFromBookMark", (object)mCPTCode);
            BtnAddToBookMark.SetActive(true);
            BtnRemoveFromBookMark.SetActive(false);
        }


        //  Private Methods  ----------------------------------------
        //
        private Sprite CreateSprite(Texture2D texture, string name)
        {
            var sprite = Sprite.Create(
                texture,
                new Rect(0.0f, 0.0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                1
            );

            sprite.name = name;
            return sprite;
        }



        //
        // Helper Functions.
        //
        // http://www.csharphelper.com/howtos/howto_file_size_in_words.html
        //
        public static string ToFileSize(double value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB",
            "TB", "PB", "EB", "ZB", "YB"};
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Mathf.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value /
                        Mathf.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(value /
                Mathf.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        // Return the value formatted to include at most three
        // non-zero digits and at most two digits after the
        // decimal point. Examples:
        //         1
        //       123
        //        12.3
        //         1.23
        //         0.12
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        /*
         * IEnumerator coDownloadSprite()
        {
            string URL = //"https://lh3.googleusercontent.com/drive-viewer/AITFw-wmmPjYLav2VBYY7ZGRq3fydMSSP4xwnKaLK2f56JtC9-ecDVg1UzXrJbL5q2F5CsPRxHQOZqOwEH2-2IM94miSRylE=s2560";
            // https://drive.google.com/file/d/1GnwQ-bEUdS98f3v9ptM4yAQWKoin6awe/view
            // "https://drive.google.com/uc?id=1GnwQ-bEUdS98f3v9ptM4yAQWKoin6awe";
             "https://image.fmkorea.com/filesn/cache/thumbnails/20230911/874/883/170/006/70x50.crop.jpg?c=20230911110130";


            //using (UnityWebRequest req = UnityWebRequest.Get(URL))
            using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(URL, true))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(req.error);
                }
                else
                {
                    var result = ((DownloadHandlerTexture)req.downloadHandler)?.texture;
                    ImgIcon.sprite = CreateSprite(result, "AAA");
                }
            }
        }
        */
    }
}
