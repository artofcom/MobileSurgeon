using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Core.Tween
{

    public class Zoom2D : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        enum MODE { ZOOM, DRAG }

        // Serializer ---------------------------------
        //
        [SerializeField] Transform TransformTarget;
        [SerializeField] float ScreenZoomRate = 10.0f;
        [SerializeField] float ScreenDragRate = 1.0f;
        [SerializeField] float ResetTweenDuration = 0.2f;

        [Space(10)]
        [SerializeField] MODE mode = MODE.ZOOM;


        [Space(10)]
        [SerializeField] Button ButtonScaleUp;
        [SerializeField] Button ButtonScaleDown;
        [SerializeField] Button ButtonReset;
        [SerializeField] float ButtonZoomRate = 0.5f;


        // Member values ---------------------------------
        //
#if !UNITY_EDITOR
        bool mIsTouching = false;
#endif
        bool mIsDragging = false;
        Vector3 vOrigin, vDiffZoom, vDiffDrag;
        Vector3 vOrgScale, vOrgPos;
        bool mIsZooming = false;
        float mBtnDownTime;

        // Unity Overrides ---------------------------------
        //
        void Start()
        {
            ButtonScaleUp.onClick.AddListener(OnZoomInClicked);
            ButtonScaleDown.onClick.AddListener(OnZoomOutClicked);
        }

        void Update()
        {
#if UNITY_EDITOR
            UpdateDragStatus();
            mIsZooming = mode == MODE.ZOOM;
            UpdateZoom();
            UpdateDrag();

            vDiffZoom = Vector3.zero;
#else
            UpdateTouchStatusDevice();
            if (!mIsTouching) return;

            UpdateZoom();
            UpdateDrag();

#endif

            // TryReset();
        }

        private void OnEnable()
        {
            mIsDragging = false;
#if !UNITY_EDITOR
            mIsTouching = false;
#endif

            StartCoroutine(coInit());

            vDiffZoom = Vector3.zero;
            vDiffDrag = Vector3.zero;
            mode = MODE.ZOOM;

            bool isOnEditor = true;
#if !UNITY_EDITOR
            isOnEditor = false;
#endif
            ButtonScaleUp.gameObject.SetActive(isOnEditor);
            ButtonScaleDown.gameObject.SetActive(isOnEditor);
            ButtonReset.interactable = false;
        }
        IEnumerator coInit()
        {
            yield return new WaitForSeconds(0.5f);

            vOrgScale = TransformTarget.localScale;
            vOrgPos = TransformTarget.localPosition;
        }
        private void OnDisable()
        {
            TransformTarget.localScale = vOrgScale;
            TransformTarget.localPosition = vOrgPos;
        }




        // Event Handlers ---------------------------------
        //
        public void OnPointerDown(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            mIsTouching = true;
#endif
            mBtnDownTime = Time.time;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            mIsTouching = false;
#endif
            Core.Events.EventSystem.DispatchEvent("Zoom2D_OnPointerUp", Time.time - mBtnDownTime);
        }
        // Editor Only Funcs.
        public void OnZoomInClicked()
        {
            vDiffZoom = new Vector3(vDiffZoom.x, vDiffZoom.y + ButtonZoomRate, vDiffZoom.z);
            Core.Events.EventSystem.DispatchEvent("Zoom2D_OnZoomUpdated");
            ButtonReset.interactable = true;
            TryReset();
        }
        public void OnZoomOutClicked()
        {
            vDiffZoom = new Vector3(vDiffZoom.x, vDiffZoom.y - ButtonZoomRate, vDiffZoom.z);
            Core.Events.EventSystem.DispatchEvent("Zoom2D_OnZoomUpdated");
            ButtonReset.interactable = true;
            TryReset();
        }
        public void OnZoomResetClicked()
        {
            TransformTarget.localScale = vOrgScale;
            TransformTarget.localPosition = vOrgPos;
            ButtonReset.interactable = false;
            Core.Events.EventSystem.DispatchEvent("Zoom2D_OnZoomReset");
        }


        // Private members ---------------------------------
        //
        void UpdateDragStatus()
        {
            // Editor Only Func. 
#if UNITY_EDITOR

            // Check if this is draggable.
            if (TransformTarget.localScale == vOrgScale)
                return;

            if (Input.GetMouseButton(0))
            {
                if (mIsDragging == false)
                {
                    mIsDragging = true;
                    vOrigin = Input.mousePosition;
                }
                vDiffDrag = Input.mousePosition - vOrigin;
            }
            else
            {
                if (mIsDragging)
                    TryReset();

                mIsDragging = false;
                vDiffDrag = Vector3.zero;
            }

            if (mIsDragging == true)
                vOrigin = Input.mousePosition;
#endif
        }

        void UpdateTouchStatusDevice()
        {
            // Device Only Func.
            //
#if !UNITY_EDITOR
        
            //
            // Dragging Part  -----------------------------------
            // 
            if (Input.GetMouseButton(0) && Input.touchCount == 1)
            {
                if (mIsDragging == false)
                {
                    mIsDragging = true;
                    vOrigin = Input.mousePosition;
                }
                vDiffDrag = Input.mousePosition - vOrigin;
            }
            else
            {
                if(mIsDragging)
                    TryReset();

                mIsDragging = false;
                vDiffDrag = Vector3.zero;
            }

            if (mIsDragging == true)
                vOrigin = Input.mousePosition;



            // Zoom In/Out Part  -----------------------------------
            // 
            // checkin MULTI touch for zoomming.
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Calculating zoom pinch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
                vDiffZoom = new Vector3(.0f, currentMagnitude - prevMagnitude, .0f);
                mIsZooming = true;

                ButtonReset.interactable = true;
                Core.Events.EventSystem.DispatchEvent("Zoom2D_OnZoomUpdated");
            }
            else
            {
                if (mIsZooming)
                    TryReset();

                vDiffZoom = Vector3.zero;
                mIsZooming = false;
            }
#endif
        }

        void TryReset()
        {
            StartCoroutine(coTriggerActionWithDelay(0.01f, () =>
            {
                // Smaller than original size? - then tween to original.
                if (TransformTarget.localScale.magnitude < vOrgScale.magnitude)
                {
                    StartCoroutine(coTweenMove(TransformTarget, TransformTarget.localPosition, vOrgPos, ResetTweenDuration));
                    StartCoroutine(coTweenScale(TransformTarget, TransformTarget.localScale, vOrgScale, ResetTweenDuration));

                    StartCoroutine(coTriggerActionWithDelay(ResetTweenDuration, () =>
                    {
                        Core.Events.EventSystem.DispatchEvent("Zoom2D_OnZoomReset");
                    }));
                }
            }));
        }


        IEnumerator coTweenMove(Transform trTarget, Vector3 vStart, Vector3 vEnd, float duration)
        {
            trTarget.localPosition = vStart;

            float fStartT = Time.time;
            while (Time.time < fStartT + duration)
            {
                float timeDelta = Mathf.Clamp01((Time.time - fStartT) / duration);// durationEaseFunc(Time.time - fStartT, duration);
                trTarget.localPosition = Vector3.LerpUnclamped(vStart, vEnd, timeDelta);

                yield return null;
            }
            trTarget.localPosition = vEnd;
        }
        IEnumerator coTweenScale(Transform trTarget, Vector3 vStart, Vector3 vEnd, float duration)
        {
            trTarget.localScale = vStart;

            float fStartT = Time.time;
            while (Time.time < fStartT + duration)
            {
                float timeDelta = Mathf.Clamp01((Time.time - fStartT) / duration);// durationEaseFunc(Time.time - fStartT, duration);
                trTarget.localScale = Vector3.LerpUnclamped(vStart, vEnd, timeDelta);

                yield return null;
            }
            trTarget.localScale = vEnd;
        }
        IEnumerator coTriggerActionWithDelay(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);

            action.Invoke();
        }

        void UpdateZoom()
        {
            if (!mIsZooming) return;

            float diffRate = 1.0f + vDiffZoom.y * ScreenZoomRate;
            TransformTarget.localScale = new Vector3(TransformTarget.localScale.x * diffRate, TransformTarget.localScale.y * diffRate, TransformTarget.localScale.z);
        }

        void UpdateDrag()
        {
            if (!mIsDragging) return;

            TransformTarget.localPosition = new Vector3(
                TransformTarget.localPosition.x + vDiffDrag.x * ScreenDragRate,
                TransformTarget.localPosition.y + vDiffDrag.y * ScreenDragRate, TransformTarget.localPosition.z);
        }
    }
}

/*
 * void UpdateTouchStatus()
    {
#if !UNITY_EDITOR
        // Editor Only Func. 
        return;
#endif
        // Check if this is draggable.
        if (TransformTarget.localScale == vOrgScale)
            return;

        if (Input.GetMouseButton(0))
        {
            if (mIsDragging == false)
            {
                mIsDragging = true;
                mIsZooming = true;
                vOrigin = Input.mousePosition;
            }
            vDiffDrag = Input.mousePosition - vOrigin;
            vDiffZoom = Input.mousePosition - vOrigin;
        }
        else
        {
            mIsDragging = false;
            mIsZooming = false;
            vDiffDrag = Vector3.zero;
            vDiffZoom = Vector3.zero;
        }

        if (mIsDragging == true)
            vOrigin = Input.mousePosition;
    }
 */