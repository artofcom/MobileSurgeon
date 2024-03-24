using UnityEngine;
using TMPro;
using System.Collections;

namespace App.MVCS
{
    public class SurgeLoadingView : AView
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] RectTransform ProgressBar;
        [SerializeField] GameObject ObjLoadingShrug;
        [SerializeField] GameObject ObjProgressBarGroup;
        [SerializeField] GameObject ObjLoginGroup;
        [SerializeField] GameObject ObjSignUpGroup;

        [SerializeField] TMP_InputField InputLoginID;
        [SerializeField] TMP_InputField InputLoginPW;

        [SerializeField] TMP_InputField InputSignupID;
        [SerializeField] TMP_InputField InputSignupPW;


        public string LoginID => InputLoginID.text;
        public string LoginPassword => InputLoginPW.text;
        public string SignUpID => InputSignupID.text;
        public string SignUpPassword => InputSignupPW.text;


        public enum STATE
        {
            TRY_AUTO_SESSIONLOGIN,
            WAIT_FOR_INPUT,
            SIGN_UP,
            TRY_SIGN_IN,
            SIGN_IN_SUCCESSED,
        }
        STATE mEState = STATE.TRY_AUTO_SESSIONLOGIN;


        //  Unity Event Handlers ------------------------------
        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(ProgressBar != null);
            UnityEngine.Assertions.Assert.IsTrue(ObjLoadingShrug != null);
            UnityEngine.Assertions.Assert.IsTrue(InputLoginID != null);
            UnityEngine.Assertions.Assert.IsTrue(InputLoginPW != null);
            UnityEngine.Assertions.Assert.IsTrue(InputSignupID != null);
            UnityEngine.Assertions.Assert.IsTrue(InputSignupPW != null);
        }
        private void OnEnable()
        {
            StartCoroutine(TriggerActionWithDelay(0.1f, () => Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnEnable") ));
        }
        private void OnDisable()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnDisable");
        }
        public void SetViewState(STATE state)
        {
            switch(state)
            {
                case STATE.TRY_AUTO_SESSIONLOGIN:
                    ObjLoadingShrug.SetActive(true);
                    ObjProgressBarGroup.SetActive(false);
                    ObjSignUpGroup.SetActive(false);
                    ObjLoginGroup.SetActive(false);
                    break;
                case STATE.WAIT_FOR_INPUT:
                    ObjLoadingShrug.SetActive(false);
                    ObjProgressBarGroup.SetActive(false);
                    ObjSignUpGroup.SetActive(false);
                    ObjLoginGroup.SetActive(true);
                    break;
                case STATE.TRY_SIGN_IN:
                    ObjLoadingShrug.SetActive(true);
                    break;
                case STATE.SIGN_UP:
                    ObjLoadingShrug.SetActive(false);
                    ObjProgressBarGroup.SetActive(false);
                    ObjSignUpGroup.SetActive(true);
                    ObjLoginGroup.SetActive(false);
                    break;
                case STATE.SIGN_IN_SUCCESSED:
                    StartCoroutine(coUpdateProgressBar());
                    ObjProgressBarGroup.SetActive(true);
                    ObjSignUpGroup.SetActive(false);
                    ObjLoginGroup.SetActive(false);
                    break;
                default:
                    return;
            }
            mEState = state;
        }

        public void RefreshProgressBar(float progress)
        {
            ProgressBar.transform.localScale = new Vector3(progress, 1.0f, 1.0f);
        }

        public void OnBtnContinueClicked()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnBtnContinueClicked");
        }

        public void OnBtnSignUpClicked()
        {
            SetViewState(STATE.SIGN_UP);
        }

        public void OnBtnSignUpCloseClicked()
        {
            SetViewState(STATE.WAIT_FOR_INPUT);
        }

        public void ClearInputField()
        {
            InputLoginID.text = string.Empty;
            InputLoginPW.text = string.Empty;
            InputSignupID.text = string.Empty;
            InputSignupPW.text = string.Empty;
        }

        public void OnBtnSignUpSubmitClicked()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnBtnSignUpSubmitClicked");
        }

        public void OnBtnGuestLoginClicked()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnBtnGuestLoginClicked");

            ObjProgressBarGroup.SetActive(true);
            ObjLoginGroup.SetActive(false);
        }

        public void OnBtnGoogleLoginClicked()
        {
            //Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnEnable");
        }

        public void OnBtnFaceBookGuestLoginClicked()
        {
            //Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnEnable");
        }

        IEnumerator coUpdateProgressBar()
        {
            const float LoadingViewMinTime = 0.5f;
            float fStartTime = Time.time;

            while (Time.time - fStartTime <= LoadingViewMinTime)
            {
                float progress = (Time.time - fStartTime) / LoadingViewMinTime;
                RefreshProgressBar(progress);
                yield return null;
            }
            Core.Events.EventSystem.DispatchEvent("SurgeLoading_OnSignInSuccessed", (object)true);
        }

        IEnumerator TriggerActionWithDelay(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);

            action.Invoke();
        }
    }
}
