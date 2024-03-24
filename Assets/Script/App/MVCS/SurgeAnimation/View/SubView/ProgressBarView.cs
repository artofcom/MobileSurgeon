using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Core.Events;

namespace App.MVCS
{
    public class ProgressBarView : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] RectTransform ProgressStart, ProgressEnd;


        //Properties ------------------------------------
        //
        float mXMin, mXMax;
        public bool ExpertMode { set; private get; }
        float mMSDownX, mMSUpX;

        // Mono Callbacks--------------------------------
        //
        void Start()
        {
        }

        void OnEnable()
        {
            Vector3 vMin = transform.parent.TransformPoint(new Vector3(ProgressStart.localPosition.x, .0f, .0f));
            Vector3 vMax = transform.parent.TransformPoint(new Vector3(ProgressEnd.localPosition.x, .0f, .0f));

            mXMin = vMin.x;
            mXMax = vMax.x;
        }

        // Event Handlers--------------------------------
        //
        public void OnDrag(PointerEventData eventData)
        {
            if (!ExpertMode) return;

            float fRate = (eventData.position.x - mXMin) / (mXMax - mXMin);
            fRate = Mathf.Max(.0f, fRate);
            Core.Events.EventSystem.DispatchEvent("OnProgressBarDragged", (object)fRate);

            // Debug.Log("Pointer is Dragging...." + fRate);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!ExpertMode) return;

            mMSDownX = eventData.position.x;
            Debug.Log("Pointer Down. " + mMSDownX);
            Core.Events.EventSystem.DispatchEvent("OnProgressBarMSDown");
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!ExpertMode) return;

            mMSUpX = eventData.position.x;
            Debug.Log("Pointer Up. " + mMSUpX);
            Core.Events.EventSystem.DispatchEvent("OnProgressBarMSUp");


            if (Mathf.Abs(mMSUpX - mMSDownX) < 1.0f)
            {
                float fRate = (mMSUpX - mXMin) / (mXMax - mXMin);
                fRate = Mathf.Max(.0f, fRate);
                Core.Events.EventSystem.DispatchEvent("OnProgressBarClicked", (object)fRate);
            }
        }
    }

}