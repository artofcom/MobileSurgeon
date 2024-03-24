using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.MVCS
{
    public class XRayDialogLayoutView : MonoBehaviour
    {
        [SerializeField] Button[] XRayButtons;

        public void Refresh(List<Sprite> sprites, List<string> Names)
        {
            UnityEngine.Assertions.Assert.IsTrue(XRayButtons != null && XRayButtons.Length == sprites.Count);
            UnityEngine.Assertions.Assert.IsTrue(Names.Count == sprites.Count);

            for (int q = 0; q < XRayButtons.Length; ++q)
            {
                XRayButtons[q].GetComponent<Image>().sprite = sprites[q];

                // Set Name String.
                const string NameKey = "txtName";
                Transform trName = XRayButtons[q].transform.Find(NameKey);
                if (trName != null)
                {
                    TMP_Text TxtName = trName.GetComponent<TMP_Text>();
                    if (TxtName != null)
                        TxtName.text = Names[q];
                }
            }
        }
    }
}
