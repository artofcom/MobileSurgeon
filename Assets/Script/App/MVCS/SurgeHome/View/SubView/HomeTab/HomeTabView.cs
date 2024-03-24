using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Core.Events;
using System;

namespace App.MVCS
{
    public class HomeTabView : MonoBehaviour
    {
        // Serialized Field ------------------------------
        //
        [SerializeField] GameObject scrollView;

        [SerializeField] GameObject PrefabSmallItem;
        [SerializeField] GameObject PrefabBigItem;
        [SerializeField] GameObject PrefabSectionItem;
        [SerializeField] GameObject ImageLoading;



        //  Properties ------------------------------------

        List<GameObject> mListObjectItems = new List<GameObject>();


        //  Presentation Model------------------------------
        public class PresentData
        {
            //public string imagePath;
            //public int recentLearningId;

            public HomeItemDefine.PresentModel RecentLearning;

            public List<HomeItemDefine.PresentModel> listItemInfo;
        }


        //  Event Handler------------------------------------
        private void OnEnable()
        {
            ImageLoading.SetActive(true);
            StartCoroutine(coTriggerEvent());
        }


        //  Methods  -----------------------------------------
        //
        IEnumerator coTriggerEvent()
        {
            while (!EventSystem.HasEventRegistered("HomeTabView_OnEnabled"))
                yield return null;

            EventSystem.DispatchEvent("HomeTabView_OnEnabled");
        }

        public void Refresh(PresentData presentData)
        {
            // destroy old ones first.
            for (int k = 0; k < mListObjectItems.Count; ++k)
                GameObject.Destroy(mListObjectItems[k]);

            ScrollRect rt = scrollView.GetComponent<ScrollRect>();
            if (presentData.RecentLearning != null && presentData.RecentLearning.CPTCode > 0)
            {
                GameObject prefab = PrefabBigItem;
                var obj = GameObject.Instantiate(prefab, rt.content.transform);

                HomeItemBigView.Data data = new HomeItemBigView.Data();
                data.CPTCode = presentData.RecentLearning.CPTCode;
                data.ImagePath = presentData.RecentLearning.ImagePath;
                data.TagName = presentData.RecentLearning.Tag;
                data.Title = $"CPT {presentData.RecentLearning.CPTCode}";
                data.Desc = presentData.RecentLearning.Desc;
                obj.GetComponent<HomeItemBigView>().Refersh(data);

                mListObjectItems.Add(obj);
            }


            if (presentData.listItemInfo != null)
            {
                for (int k = 0; k < presentData.listItemInfo.Count; ++k)
                {
                    HomeItemDefine.PresentModel info = presentData.listItemInfo[k];
                    // Debug.Log($"{surgInfo.Name}");

                    GameObject obj = null;
                    if (info.Type == "BIG")
                    {
                        obj = GameObject.Instantiate(PrefabBigItem, rt.content.transform);
                        HomeItemBigView.Data data = new HomeItemBigView.Data();
                        data.CPTCode = info.CPTCode;
                        data.ImagePath = info.ImagePath;
                        data.Title = $"CPT {info.CPTCode}";
                        data.Desc = info.Desc;
                        data.TagName = info.Tag;
                        obj.GetComponent<HomeItemBigView>().Refersh(data);
                    }
                    else if (info.Type == "SECTION")
                    {
                        obj = GameObject.Instantiate(PrefabSectionItem, rt.content.transform);
                        obj.GetComponent<HomeSectionItemView>().Refresh(info.Tag);
                    }
                    else
                    {
                        obj = GameObject.Instantiate(PrefabSmallItem, rt.content.transform);
                        HomeItemView.Data data = new HomeItemView.Data();
                        data.CPTCode = info.CPTCode;
                        data.ImagePath = info.ImagePath;
                        data.Title = string.IsNullOrEmpty(info.Tag) ? $"CPT {info.CPTCode}" : $"{info.Tag} CPT {info.CPTCode}";
                        data.Desc = info.Desc;
                        data.CanDeleteFromBookmark = info.CanRemoveFromBookmark;

                        obj.GetComponent<HomeItemView>().Refersh(data);
                    }
                    mListObjectItems.Add(obj);
                }
            }

            ImageLoading.SetActive(false);
        }
    }
}
