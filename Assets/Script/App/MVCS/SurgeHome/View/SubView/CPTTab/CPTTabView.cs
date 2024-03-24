using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Events;

namespace App.MVCS
    {
    public class CPTTabView : MonoBehaviour
    {
        //  Properties  ----------------------------------------
        //
        [SerializeField] GameObject scrollView;
        [SerializeField] GameObject PrefabListItem;
        [SerializeField] TMP_InputField SearchField;

        [SerializeField] Button NameSortButton;
        [SerializeField] Button CPTSortButton;
        [SerializeField] Button RVUSortButton;

        [SerializeField] TMP_Text NameSortBtnName;
        [SerializeField] TMP_Text CPTSortBtnName;
        [SerializeField] TMP_Text RVUSortBtnName;

        [SerializeField] Color SelectedBtnColor;
        [SerializeField] Color NormalBtnColor;

        List<GameObject> mListObjectItems = new List<GameObject>();
        List<SurgListItemView.PresentData> mListSurgeInfo = new List<SurgListItemView.PresentData>();

        public class PresentData
        {
            public List<SurgListItemView.PresentData> ItemPresentData;
            public CPTTabController.eSORT_TYPE SortType;
            public bool bAscendingSort;
        }

        //  Mono Event Handlers  ----------------------------------------
        //
        //void Start(){ }
        private void OnEnable()
        {
            EventSystem.DispatchEvent("CPTTabView_OnEnabled");
        }
        private void OnDisable()
        {
            EventSystem.DispatchEvent("CPTTabView_OnDisabled");
        }

        //  Public Methods  ----------------------------------------
        //
        public void Refresh(PresentData data)
        {
            // destroy old ones first.
            for (int k = 0; k < mListObjectItems.Count; ++k)
                GameObject.Destroy(mListObjectItems[k]);

            // caching.
            if (data.ItemPresentData != null)
            {
                mListSurgeInfo.Clear();
                for (int k = 0; k < data.ItemPresentData.Count; ++k)
                    mListSurgeInfo.Add(data.ItemPresentData[k]);
            }

            NameSortButton.GetComponent<Image>().color = NormalBtnColor;
            CPTSortButton.GetComponent<Image>().color = NormalBtnColor;
            RVUSortButton.GetComponent<Image>().color = NormalBtnColor;

            string btnCPTSortName = "CPT";
            string btnRVUSortName = "RVU";
            string btnNameSortName = "Name";
            switch (data.SortType)
            {
                case CPTTabController.eSORT_TYPE.CPT:
                default:
                    // CPT Code sort.
                    btnCPTSortName += (data.bAscendingSort ? " v" : " ^");
                    mListSurgeInfo.Sort((a, b) => { return SortByCPTCode(a, b, data.bAscendingSort); });
                    CPTSortButton.GetComponent<Image>().color = SelectedBtnColor;
                    break;

                case CPTTabController.eSORT_TYPE.RVU:
                    // RVU sort.
                    btnRVUSortName += (data.bAscendingSort ? " v" : " ^");
                    mListSurgeInfo.Sort((a, b) => { return SortByRVU(a, b, data.bAscendingSort); });
                    RVUSortButton.GetComponent<Image>().color = SelectedBtnColor;
                    break;

                case CPTTabController.eSORT_TYPE.NAME:
                    // alphabetical sort.
                    btnNameSortName += (data.bAscendingSort ? " v" : " ^");
                    mListSurgeInfo.Sort((a, b) => { return SortByName(a, b, data.bAscendingSort); });
                    NameSortButton.GetComponent<Image>().color = SelectedBtnColor;
                    break;
            }

            CPTSortBtnName.text = btnCPTSortName;
            RVUSortBtnName.text = btnRVUSortName;
            NameSortBtnName.text = btnNameSortName;

            // Listing item should be almost same. But should be sorted alphabetically ?
            ScrollRect rt = scrollView.GetComponent<ScrollRect>();
            for (int k = 0; k < mListSurgeInfo.Count; ++k)
            {
                var presentData = mListSurgeInfo[k];

                string textBuff = $"CPT {presentData.CPTCode} {presentData.Name} {presentData.Desc} RUV {presentData.RVU}";
                if (SearchField.text.Length > 0 && !textBuff.ToLower().Contains(SearchField.text.ToLower()))
                    continue;

                var obj = Instantiate(PrefabListItem, rt.content.transform);
                obj.GetComponent<SurgListItemView>().Refresh(presentData);
                mListObjectItems.Add(obj);
            }
        }



        //  Event Handlers  ----------------------------------------
        //
        public void OnSearchCancelClicked()
        {
            SearchField.text = "";
            EventSystem.DispatchEvent("CPTTabView_OnSearchFieldChanged");
        }
        public void OnSearchFieldChanged()
        {
            EventSystem.DispatchEvent("CPTTabView_OnSearchFieldChanged");
        }
        public void OnCPTSortClicked()
        {
            EventSystem.DispatchEvent("CPTTabView_OnCPTSortClicked");
        }
        public void OnRVUSortClicked()
        {
            EventSystem.DispatchEvent("CPTTabView_OnRVUSortClicked");
        }
        public void OnNameSortClicked()
        {
            EventSystem.DispatchEvent("CPTTabView_OnNameSortClicked");
        }

        //  Helper  ----------------------------------------
        //
        int SortByCPTCode(SurgListItemView.PresentData a, SurgListItemView.PresentData b, bool ascendingSort)
        {
            if (a.CPTCode < b.CPTCode) return ascendingSort ? 1 : -1;
            else if (a.CPTCode > b.CPTCode) return ascendingSort ? -1 : 1;
            return 0;
        }
        int SortByRVU(SurgListItemView.PresentData a, SurgListItemView.PresentData b, bool ascendingSort)
        {
            if (a.RVU < b.RVU) return ascendingSort ? 1 : -1;
            else if (a.RVU > b.RVU) return ascendingSort ? -1 : 1;
            return 0;
        }
        int SortByName(SurgListItemView.PresentData a, SurgListItemView.PresentData b, bool ascendingSort)
        {
            char one = char.ToLower(a.Desc[0]);
            char two = char.ToLower(b.Desc[0]);
            if (one < two) return ascendingSort ? 1 : -1;
            else if (one > two) return ascendingSort ? -1 : 1;
            return 0;
        }
    }
}


// alphabetical sort.
/*mListSurgeInfo.Sort((a, b) =>
{
    char one = char.ToLower(a.Name[0]);
    char two = char.ToLower(b.Name[0]);
    if (one < two) return -1;
    else if (one > two) return 1;
    return 0;
});*/