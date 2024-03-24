using UnityEngine;
using TMPro;
using Core.Events;
using Core.WWW;

namespace App.MVCS
{
    public class SpecialtyCategoryItemView : MonoBehaviour
    {
        //  Properties  ----------------------------------------
        //
        [SerializeField] TMP_Text Title;
        [SerializeField] UnityEngine.UI.Image ImgIcon;
        [SerializeField] GameObject ImageLoading;

        //  Constants -----------------------------------------
        //
        const string FALLBACK_PATH = "UX/Icon/fallback";


        public class PresentData
        {
            public string CategoryName, IconPath;
        }

        // Start is called before the first frame update
        void Start()
        {
            var btn = GetComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(this.OnClick);
        }

        public void Refresh(PresentData data)
        {
            Title.text = data.CategoryName;


            string picURL = data.IconPath;
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
            //var surgeListInfo = BootStrap.GetInstance().SurgeListInfo;
            //string id = surgeListInfo.SurgeryList[mIndex].Id;
            //Debug.Log($"Button {id} Clciked!");
            EventSystem.DispatchEvent("OnSpecialtyCategoryItemClicked", (object)Title.text);
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
    }
}
