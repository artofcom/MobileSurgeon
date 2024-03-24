using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Core.Events;
using System;
using System.Collections;

namespace App.MVCS
    {
    public class SurgeTopUIView : MonoBehaviour
    {
        // [SerializeField] ------------------------------
        [SerializeField] TMP_Text txtMode, txtProgress, txtExtraInfo, txtTitle;
        [SerializeField] RectTransform ProgressBar;
        [SerializeField] GameObject PrefabNote;
        [SerializeField] Transform NoteRoot;
        [SerializeField] RectTransform ProgressStart, ProgressEnd;
        [SerializeField] ProgressBarView ProgressBarBG;
        [SerializeField] RectTransform ProgressPoint;
        [SerializeField] Image ImagePreview;
        [SerializeField] RectTransform ThumbnailMinX, ThumbnailMaxX;
        [SerializeField] TMP_Text txtPrevTime;

        [SerializeField] GameObject ObjectSectorSplitter;
        [SerializeField] GameObject ObjectGroupSplitter;
        [SerializeField] TMP_Text txtGroupName;
        [SerializeField] TMP_Text txtAssetDownloadStatus;
        [SerializeField] GameObject ControlButtonGroup;

        // On/Off for now.
        [SerializeField] TMP_Text txtMessageXRay;
        [SerializeField] TMP_Text txtMessageTransparency;

        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();


        // Properties ------------------------------------
        List<GameObject> mListNoteObjs = new List<GameObject>();
        Vector2 NoteAreaX;
        bool IsExpertMode;
        // Coroutine mCoPreviewFader = null;
        List<GameObject> mListSectorSplitters = new List<GameObject>();
        List<GameObject> mListGroupSplitters = new List<GameObject>();

        List<float> mGroupSplitRates = new List<float>();   // 0 ~ 1.0f
        List<string> mGroupNames = new List<string>();

        // Present Model Def -----------------------------
        public class PresentData
        {
            public string Title;
            public int CurSector, TotalSector;
            public float CurTime, TotalTime;
            public bool IsExpertMode;
            public List<float> SectionSplitter = new List<float>(); // 0 ~ 1.0f
            public List<float> GroupSplitter = new List<float>();   // 0 ~ 1.0f
            public List<string> GroupNames = new List<string>();
        }

        public class PresentData4Note
        {
            public List<float> Timings;
        }


        // Unity Methodss ---------------------------------
        //
        void Start()
        {
            // Events.RegisterEvent("PreviewCamera_OnBeginCameraRender", PreviewCamera_OnBeginCameraRender);

            Events.RegisterEvent("OnProgressBarDragged", OnProgressBarDragged);
            Events.RegisterEvent("OnProgressBarMSUp", OnProgressBarMSUp);
            Events.RegisterEvent("OnBackgroundAssetDownloadingProgress", OnBackgroundAssetDownloadingProgress);
            Events.RegisterEvent("OnBackgroundAssetDownloadingFinished", OnBackgroundAssetDownloadingFinished);
        }
        private void OnDisable()
        {
            Clear();
        }
        void Clear()
        {
            if (ImagePreview != null)
                ImagePreview.material = null;

            for (int q = 0; q < mListSectorSplitters.Count; ++q)
                GameObject.Destroy(mListSectorSplitters[q]);
            for (int q = 0; q < mListGroupSplitters.Count; ++q)
                GameObject.Destroy(mListGroupSplitters[q]);
            mListSectorSplitters.Clear();
            mListGroupSplitters.Clear();
        }


        // Event Handler ---------------------------------
        //
        /*void PreviewCamera_OnBeginCameraRender(object data)
        {
            // no valid preview UI Image.
            if (ImagePreview == null) 
                return;

            // Valid target.
            if (ImagePreview.material != null && ImagePreview.material.name.Contains("Default"))
            {
                ImagePreview.material = (Material)data;
            }
        }*/
        public void OnThumbnailPreviewClicked()
        {
            EventSystem.DispatchEvent("SurgeTopUIView_OnThumbnailPreviewClicked");
        }
        void OnProgressBarDragged(object data)
        {
            txtGroupName.enabled = true;
        }
        void OnProgressBarMSUp(object data)
        {
            txtGroupName.enabled = false;
        }
        void OnBackgroundAssetDownloadingProgress(object data)
        {
            txtAssetDownloadStatus.gameObject.SetActive(true);

            txtAssetDownloadStatus.text = (string)data;
        }
        void OnBackgroundAssetDownloadingFinished(object data)
        {
            txtAssetDownloadStatus.gameObject.SetActive(false);
        }

        // Public Methodss ---------------------------------
        //
        public void StartView(PresentData data, PresentData4Note noteData)
        {
            UnityEngine.Assertions.Assert.IsTrue(data.GroupSplitter.Count > 0);
            UnityEngine.Assertions.Assert.IsTrue(data.SectionSplitter.Count > 0);
            UnityEngine.Assertions.Assert.IsTrue(data.GroupNames.Count == data.GroupSplitter.Count);

            Clear();

            NoteAreaX = new Vector2(ProgressStart.localPosition.x, ProgressEnd.localPosition.x);

            ProgressBarBG.ExpertMode = data.IsExpertMode;

            //ImagePreview.transform.parent.GetComponent<ImageFader>().FadeOutTo0(0.01f);
            //Transform previewRoot = ImagePreview.transform.parent;
            //previewRoot.gameObject.SetActive(false);


            // Init Section Splitters.
            for (int q = 1; q < data.SectionSplitter.Count; ++q)
            {
                var sectorSplitter = GameObject.Instantiate(ObjectSectorSplitter, ObjectSectorSplitter.transform.parent);
                float xPos = Mathf.Lerp(ProgressStart.localPosition.x, ProgressEnd.localPosition.x, data.SectionSplitter[q]);
                sectorSplitter.gameObject.SetActive(true);
                sectorSplitter.transform.localPosition = new Vector3(xPos, sectorSplitter.transform.localPosition.y, sectorSplitter.transform.localPosition.z);
                mListSectorSplitters.Add(sectorSplitter);
            }
            // Init Group Splitters.
            for (int q = 1; q < data.GroupSplitter.Count; ++q)
            {
                var groupSplitter = GameObject.Instantiate(ObjectGroupSplitter, ObjectGroupSplitter.transform.parent);
                float xPos = Mathf.Lerp(ProgressStart.localPosition.x, ProgressEnd.localPosition.x, data.GroupSplitter[q]);
                groupSplitter.gameObject.SetActive(true);
                groupSplitter.transform.localPosition = new Vector3(xPos, groupSplitter.transform.localPosition.y, groupSplitter.transform.localPosition.z);
                mListGroupSplitters.Add(groupSplitter);
            }

            txtGroupName.enabled = false;
            txtGroupName.text = data.GroupNames[0];

            txtAssetDownloadStatus.gameObject.SetActive(false);

            // cache.
            mGroupSplitRates.Clear();
            mGroupNames.Clear();
            for (int q = 0; q < data.GroupSplitter.Count; ++q)
            {
                mGroupSplitRates.Add(data.GroupSplitter[q]);
                mGroupNames.Add(data.GroupNames[q]);
            }
            IsExpertMode = data.IsExpertMode;




            RefreshUX(data);

            RefreshNoteUX(noteData);
        }

        public void RefreshUX(PresentData data)
        {
            txtTitle.text = data.Title;
            txtMode.text = data.IsExpertMode ? "Expert Mode" : "Beginner Mode";
            txtProgress.text = $"{data.CurSector} / {data.TotalSector}";
            txtExtraInfo.text = $"{TimeToString(data.CurTime)} / {TimeToString(data.TotalTime)}";

            float fRate = data.TotalTime > float.Epsilon ? (float)(data.CurTime / data.TotalTime) : .0f;
            if (ProgressPoint != null)   // Single Point Sprite Display Mode.
            {
                float xPos = Mathf.Lerp(ProgressStart.localPosition.x, ProgressEnd.localPosition.x, fRate);
                ProgressPoint.transform.localPosition = new Vector3(xPos, ProgressPoint.transform.localPosition.y, ProgressPoint.transform.localPosition.z);
            }
            else                         // Bar progress display mode.
            {
                // ProgressBar.transform.localScale = new Vector3(fRate, 1.0f, 1.0f);
                ProgressBar.GetComponent<Image>().fillAmount = fRate;
            }

            // set group text.
            int groupIdx = -1;
            for (int k = 1; k < mGroupSplitRates.Count; ++k)
            {
                if (fRate >= mGroupSplitRates[k - 1] && fRate < mGroupSplitRates[k])
                {
                    groupIdx = k - 1;
                    break;
                }
            }
            groupIdx = groupIdx < 0 ? mGroupSplitRates.Count - 1 : groupIdx;
            txtGroupName.text = mGroupNames[groupIdx];
        }

        public void RefreshNoteUX(PresentData4Note data)
        {
            for (int k = 0; k < mListNoteObjs.Count; ++k)
                GameObject.Destroy(mListNoteObjs[k]);
            mListNoteObjs.Clear();

            if (!IsExpertMode) return;

            if (data.Timings == null || data.Timings.Count == 0)
                return;

            for (int k = 0; k < data.Timings.Count; ++k)
            {
                var obj = GameObject.Instantiate(PrefabNote, NoteRoot);

                float fX = NoteAreaX.x + (NoteAreaX.y - NoteAreaX.x) * data.Timings[k];
                obj.transform.localPosition = new Vector3(fX, obj.transform.localPosition.y + ProgressBar.localPosition.y, obj.transform.localPosition.z);
                obj.GetComponent<NoteItemView>().Init(data.Timings[k]);
                mListNoteObjs.Add(obj);
            }
        }

        public void TriggerXRayMessage(string message)
        {
            //bool isValid = !string.IsNullOrEmpty(message);
            bool isValid = false;   // Disabled by Design. - 12.26.2023
            txtMessageXRay.gameObject.SetActive(isValid);
            if (isValid)
                txtMessageXRay.text = message;
        }
        public void TriggerTransparencyMessage(string message)
        {
            // bool isValid = !string.IsNullOrEmpty(message);
            bool isValid = false;   // Disabled by Design. - 12.26.2023
            txtMessageTransparency.gameObject.SetActive(isValid);
            if (isValid)
                txtMessageTransparency.text = message;
        }
        public void SetControlButtonGroupVisibility(bool visibility)
        {
            ControlButtonGroup.SetActive(visibility);
        }




        public void DisplayPreview(Sprite spriteSrc, float progress, double curTime, int section)
        {
            /*
            float fX = ThumbnailMinX.localPosition.x + (ThumbnailMaxX.localPosition.x - ThumbnailMinX.localPosition.x) * progress;
            Transform previewRoot = ImagePreview.transform.parent;
            previewRoot.localPosition = new Vector3(fX, previewRoot.localPosition.y, previewRoot.localPosition.z);

            ImagePreview.sprite = spriteSrc;
            txtPrevTime.text = $"[{section}]-{TimeToString(curTime)}";
            previewRoot.gameObject.SetActive(true);
            previewRoot.GetComponent<ImageFader>().FadeInTo1(0.001f);

            if (mCoPreviewFader != null)
                StopCoroutine(mCoPreviewFader);

            mCoPreviewFader = StartCoroutine(coActionWithDelay(1.0f, () =>
                {
                    previewRoot.GetComponent<ImageFader>().FadeOutTo0(1.0f, () =>
                    {
                        previewRoot.gameObject.SetActive(false);
                    });

                    mCoPreviewFader = null;
                }
             ));
            */
        }




        // Helper Methodss ---------------------------------
        //
        IEnumerator coActionWithDelay(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);

            if (action != null)
                action.Invoke();
        }
        string TimeToString(double curTime)
        {
            float time = (float)curTime;
            int min = Mathf.Max(Mathf.FloorToInt(time / 60.0f) % 60, 0);
            int sec = Mathf.Max(Mathf.FloorToInt(time - ((float)(min * 60.0f))), 0);
            int sec00 = ((int)(time * 100.0f)) % 100;
            return string.Format("{0:D1}:{1:D2}:{2:D2}", min, sec, sec00);
        }
    }
}
