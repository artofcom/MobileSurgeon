using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using App.UX;

namespace App.MVCS
{
    public class SpecialtyTabView : MonoBehaviour
    {
        // Serialized Field ------------------------------
        //
        [SerializeField] SpecialtyCategoryListView SpecialtyCategoryListView;
        [SerializeField] SpecialtySurgeListView SpecialtySurgeListView;
        [SerializeField] TabButtonGroup TabButtons;
        [SerializeField] GameObject BtnBack;

        public class SurgePresentData
        {
            public int CPTCode;
            public string Name, IconPath, Desc;
            public float RVU;

            public string BundleName;
            public bool IsBundleCached, IsBookmarked;
            public long FileSize;
        }

        public class PresentData
        {
            public int mode;                    // 0 category, 1 - surgery.
            public string SubCategoryName;      // Upper Ext, Pelvis, Lower Ext
            public List<SpecialtyCategoryItemView.PresentData> ListPartCategoryItem;
            public List<SurgePresentData> ListSurgeData;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            EventSystem.DispatchEvent("SpecialtyTabView_OnEnabled");
        }



        public void Refresh(PresentData presentData)
        {
            if (presentData == null)
                return;

            SpecialtyCategoryListView.gameObject.SetActive(presentData.mode == 0);
            SpecialtySurgeListView.gameObject.SetActive(presentData.mode == 1);
            BtnBack.SetActive(presentData.mode == 1);

            if (presentData.mode == 0)
                SpecialtyCategoryListView.Refresh(presentData.ListPartCategoryItem);

            else if (presentData.mode == 1)
            {
                List<SpecialtySurgeListView.PresentData> listData = new List<SpecialtySurgeListView.PresentData>();

                for (int q = 0; q < presentData.ListSurgeData.Count; ++q)
                {
                    SpecialtySurgeListView.PresentData data = new SpecialtySurgeListView.PresentData();
                    data.CPTCode = presentData.ListSurgeData[q].CPTCode;
                    data.Desc = presentData.ListSurgeData[q].Desc;
                    data.IconPath = presentData.ListSurgeData[q].IconPath;
                    data.Name = presentData.ListSurgeData[q].Name;
                    data.RVU = presentData.ListSurgeData[q].RVU;
                    data.IsBookmarked = presentData.ListSurgeData[q].IsBookmarked;

                    data.BundleName = presentData.ListSurgeData[q].BundleName;
                    data.FileSize = presentData.ListSurgeData[q].FileSize;
                    data.IsBundleCached = presentData.ListSurgeData[q].IsBundleCached;

                    listData.Add(data);
                }
                SpecialtySurgeListView.Refresh(presentData.SubCategoryName, listData);
            }
        }

        public void OnTabUpperExtremityClicked()
        {
            TabButtons.TurnOnTabButton(0);
            EventSystem.DispatchEvent("OnTabCategoryClicked", (object)"Upper Extremity");
        }
        public void OnTabPelvisClicked()
        {
            TabButtons.TurnOnTabButton(1);
            EventSystem.DispatchEvent("OnTabCategoryClicked", (object)"Pelvis");
        }
        public void OnTabLowerExtremityClicked()
        {
            TabButtons.TurnOnTabButton(2);
            EventSystem.DispatchEvent("OnTabCategoryClicked", (object)"Lower Extremity");
        }
        public void OnBtnBackClicked()
        {
            EventSystem.DispatchEvent("OnSpecialtyTabBackBtnClicked");
        }
    }

}