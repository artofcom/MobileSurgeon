using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace App.MVCS
{
    public class HomeSectionItemView : MonoBehaviour
    {
        [SerializeField] TMP_Text SectionName;

        private void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(SectionName != null);
        }

        public void Refresh(string sectionName)
        {
            SectionName.text = sectionName;
        }
    }
}
