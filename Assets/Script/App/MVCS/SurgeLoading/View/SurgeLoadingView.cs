using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.MVCS
{
    public class SurgeLoadingView : AView
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] RectTransform ProgressBar;
        [SerializeField] GameObject ObjLoadingShrug;
        [SerializeField] GameObject ObjLoadingView;
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

            ObjLoadingShrug.SetActive(false);
            ObjSignUpGroup.SetActive(false);
            ObjLoginGroup.SetActive(true);
        }
        private void OnEnable()
        {
            //ObjLoadingShrug.SetActive(false);
            ObjSignUpGroup.SetActive(false);
            ObjLoginGroup.SetActive(true);
            ObjLoadingView.SetActive(false);

            Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnEnable");
        }
        private void OnDisable()
        {
            Core.Events.EventSystem.DispatchEvent("SurgeLoadingView_OnDisable");
        }


        public void SwitchToLoginViewGroup()
        {
            //ObjLoadingShrug.SetActive(false);
            ObjSignUpGroup.SetActive(false);
            ObjLoginGroup.SetActive(true);
        }
        public void SwitchToProgressViewGroup()
        {
            //ObjLoadingShrug.SetActive(true);
            ObjSignUpGroup.SetActive(false);
            ObjLoginGroup.SetActive(false);
            ObjLoadingView.SetActive(true);
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
            ObjSignUpGroup.SetActive(true);
            ObjLoginGroup.SetActive(false);
        }

        public void OnBtnSignUpCloseClicked()
        {
            ObjSignUpGroup.SetActive(false);
            ObjLoginGroup.SetActive(true);
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

            ObjLoadingView.SetActive(true);
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
    }
}
