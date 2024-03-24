using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace App.MVCS
{
    public class AnimPathSubView : MonoBehaviour
    {
        [SerializeField] List<TMP_Text> ButtonHeadNames;
        [SerializeField] List<TMP_Text> ButtonNames;
        [SerializeField] List<Image> ButtonImages;
        [SerializeField] Sprite SelectedBtnBG;
        [SerializeField] Sprite NormalBtnBG;


        public int SelectedIndex { get; set; }

        public class PreviewAnimInfo
        {
            public Animator animTarget;
            public string aniName;
        }
        public class PathInfo
        {
            public string PathName;
            public string Header;
            public List<PreviewAnimInfo> PreviewAnims = new List<PreviewAnimInfo>();
        }

        List<PathInfo> mPathInfos = null;

        // Start is called before the first frame update
        void Start()
        { }

        public void Refresh(List<PathInfo> pathInfos)
        {
            SelectedIndex = 0;

            if (ButtonNames == null)
                return;

            mPathInfos = pathInfos;

            for (int q = 0; q < pathInfos.Count; ++q)
            {
                if (q >= ButtonNames.Count)
                    continue;

                if (ButtonHeadNames[q] != null)
                    ButtonHeadNames[q].text = string.IsNullOrEmpty(pathInfos[q].Header) ? "Try" : pathInfos[q].Header;
                if (ButtonNames[q] != null)
                    ButtonNames[q].text = pathInfos[q].PathName;
                if(ButtonImages[q] != null)
                {
                    if (q == SelectedIndex)
                        ButtonImages[q].sprite = SelectedBtnBG;
                    else
                        ButtonImages[q].sprite = NormalBtnBG;
                }
            }
        }


        public void OnSelectAnimPath(int index)
        {
            UnityEngine.Assertions.Assert.IsTrue(index >= 0 && index < mPathInfos.Count);
            if (index < 0 || index >= mPathInfos.Count)
                return;

            SelectedIndex = index;
            for (int q = 0; q < ButtonImages.Count; ++q)
            {
                if (ButtonImages[q] != null)
                {
                    if (q == SelectedIndex)
                        ButtonImages[q].sprite = SelectedBtnBG;
                    else
                        ButtonImages[q].sprite = NormalBtnBG;
                }
            }

            for (int q = 0; q < mPathInfos[index].PreviewAnims.Count; ++q)
            {
                Animator animator = mPathInfos[index].PreviewAnims[q].animTarget;
                if (animator != null)
                    animator.Play(mPathInfos[index].PreviewAnims[q].aniName);
            }
        }
    }
}
