using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace App.MVCS
{
    public class SpecialtyCategoryListView : MonoBehaviour
    {
        //  Properties  ----------------------------------------
        //
        [SerializeField] GameObject scrollView;
        [SerializeField] GameObject PrefabListItem;



        List<GameObject> mListObjectItems = new List<GameObject>();


        public void Refresh(List<SpecialtyCategoryItemView.PresentData> listCategoryNames)
        {
            // destroy old ones first.
            for (int k = 0; k < mListObjectItems.Count; ++k)
                GameObject.Destroy(mListObjectItems[k]);


            ScrollRect rt = scrollView.GetComponent<ScrollRect>();
            for (int k = 0; k < listCategoryNames.Count; ++k)
            {
                var obj = GameObject.Instantiate(PrefabListItem, rt.content.transform);
                obj.GetComponent<SpecialtyCategoryItemView>().Refresh(listCategoryNames[k]);
                mListObjectItems.Add(obj);
            }

        }
    }
}
