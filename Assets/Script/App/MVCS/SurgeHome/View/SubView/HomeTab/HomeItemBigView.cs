using UnityEngine;
using TMPro;
using Core.Events;
using Core.WWW;

namespace App.MVCS
{
    public class HomeItemBigView : MonoBehaviour
    {
        [SerializeField] TMP_Text txtTitle;
        [SerializeField] TMP_Text txtDesc;
        [SerializeField] TMP_Text txtTagName;
        [SerializeField] UnityEngine.UI.Image ImgIcon;
        [SerializeField] GameObject ImageLoading;
        [SerializeField] GameObject BtnDeleteCache;

        const string FALLBACK_PATH = "UX/Icon/fallback";

        public class Data
        {
            public int CPTCode;
            public string Title, Desc, TagName;
            public string ImagePath;
        }

        int mCPTCode;

        private void Awake()
        {
            ImageLoading.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            var btn = GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(this.OnClick);
        }

        public void Refersh(Data data)
        {
            txtTagName.text = data.TagName;// "Continue Learning"; // Recommended Learning
            txtTitle.text = data.Title;
            //$"{info.Name}/({info.PhaseCount} Phase)";
            txtDesc.text = data.Desc;

            mCPTCode = data.CPTCode;

            string picURL = data.ImagePath;
            if (!string.IsNullOrEmpty(picURL) && picURL.ToLower().Contains("http"))
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
            }
        }

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

        public void OnClick()
        {
            Debug.Log($"Button CPT : {mCPTCode} Clciked!");
            EventSystem.DispatchEvent("OnSurgItemClicked", (object)mCPTCode);
        }
    }
}
