using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Events;
using TMPro;

namespace App.MVCS
{
    public class SpecialtySurgeListView : MonoBehaviour
    {
        //  Properties  ----------------------------------------
        //
        [SerializeField] GameObject scrollView;
        [SerializeField] GameObject PrefabListItem;
        [SerializeField] TMP_Text txtSubCategory;
        [SerializeField] GameObject EmptySpace;

        List<GameObject> mListObjectItems = new List<GameObject>();



        public class PresentData
        {
            public int CPTCode;
            public string Name, IconPath, Desc;
            public float RVU;

            public string BundleName;
            public bool IsBundleCached, IsBookmarked;
            public long FileSize;
        }



        //  Mono Event Handlers  ----------------------------------------
        //
        private void OnEnable()
        {
            EventSystem.DispatchEvent("SpecialtySurgeListView_OnEnabled");
        }
        private void OnDisable()
        {
            EventSystem.DispatchEvent("SpecialtySurgeListView_OnDisabled");
        }


        public void Refresh(string subCategoryName, List<PresentData> listData)
        {
            // destroy old ones first.
            for (int k = 0; k < mListObjectItems.Count; ++k)
                GameObject.Destroy(mListObjectItems[k]);


            txtSubCategory.text = subCategoryName;
            EmptySpace.SetActive(listData.Count <= 0);
            scrollView.SetActive(listData.Count > 0);

            if (scrollView.activeSelf)
            {
                ScrollRect rt = scrollView.GetComponent<ScrollRect>();
                for (int k = 0; k < listData.Count; ++k)
                {
                    var obj = GameObject.Instantiate(PrefabListItem, rt.content.transform);

                    SurgListItemView.PresentData itemData = new SurgListItemView.PresentData();
                    itemData.CPTCode = listData[k].CPTCode;
                    itemData.Desc = listData[k].Desc;
                    itemData.RVU = listData[k].RVU;
                    itemData.Name = listData[k].Name;
                    itemData.IconPath = listData[k].IconPath;
                    itemData.IsBookmarked = listData[k].IsBookmarked;

                    itemData.BundleName = listData[k].BundleName;
                    itemData.FileSize = listData[k].FileSize;
                    itemData.IsBundleCached = listData[k].IsBundleCached;

                    obj.GetComponent<SurgListItemView>().Refresh(itemData);
                    mListObjectItems.Add(obj);
                }
            }
        }
    }
}
