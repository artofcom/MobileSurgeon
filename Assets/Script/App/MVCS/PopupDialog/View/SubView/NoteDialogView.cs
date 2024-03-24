using UnityEngine;
using TMPro;

namespace App.MVCS
{
    public class NoteDialogView : PopupDialogScreen
    {
        // [SerializeField] ------------------------------
        //
        [SerializeField] GameObject EditModeRoot;
        [SerializeField] GameObject ViewModeRoot;
        [SerializeField] TMP_Text TxtContent;
        [SerializeField] GameObject InputField;


        // Properties ------------------------------------
        //
        System.Action<ReturnData> mCloseCallback = null;
        ReturnData mReturnData = new ReturnData();
        bool IsEditMode = false;


        // Data Model -----------------------------
        //
        public class PresentData : IDialogPresentData
        {
            public int index;
            public string MemoContent;
        }
        public class ReturnData : IDialogReturn
        {
            public bool ok { get; set; }
            public bool delete { get; set; }
            public bool jump { get; set; }
            public string content { get; set; }
            public void Clear()
            {
                ok = false; delete = false; jump = false; content = "";
            }
        }

        // Methods -   -----------------------------------
        //
        // Start is called before the first frame update
        void Start()
        {
            //var btn = GetComponent<UnityEngine.UI.Button>();
            //btn.onClick.AddListener(this.OnClick);
        }

        // index : note index ( -1 : new one )
        public override void Trigger(IDialogPresentData data, System.Action<IDialogReturn> closeCallBack)
        {
            gameObject.SetActive(true);
            mReturnData.Clear();

            var presentData = data as PresentData;

            IsEditMode = presentData.index < 0;
            EditModeRoot.SetActive(IsEditMode);
            ViewModeRoot.SetActive(!IsEditMode);

            // Update Mode.
            if (presentData.index >= 0)
                TxtContent.text = presentData.MemoContent;

            // New Mode.
            else
            {
                InputField.GetComponent<TMP_InputField>().text = string.Empty;
            }

            mCloseCallback = closeCallBack;
        }





        // Event Handlers  -----------------------------------
        //
        public void OnClose()
        {
            gameObject.SetActive(false);

            mReturnData.ok = false;
            mReturnData.delete = false;
            mReturnData.content = TxtContent.text;
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }

        public void OnEdit()
        {
            InputField.GetComponent<TMP_InputField>().text = TxtContent.text;

            IsEditMode = true;
            EditModeRoot.SetActive(IsEditMode);
            ViewModeRoot.SetActive(!IsEditMode);
        }

        public void OnDeleteClicked()
        {
            gameObject.SetActive(false);

            mReturnData.ok = true;
            mReturnData.delete = true;
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }

        public void OnJumpToPosition()
        {
            gameObject.SetActive(false);
            mReturnData.ok = true;
            mReturnData.delete = true;
            mReturnData.jump = true;
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }

        public void OnBtnOK()
        {
            // save this change to data and close popup.
            //
            gameObject.SetActive(false);

            mReturnData.ok = true;
            mReturnData.delete = false;
            mReturnData.content = TxtContent.text;
            if (mCloseCallback != null)
                mCloseCallback.Invoke(mReturnData);
        }

        public void OnEditDone()
        {
            TxtContent.text = InputField.GetComponent<TMP_InputField>().text;

            IsEditMode = false;
            EditModeRoot.SetActive(IsEditMode);
            ViewModeRoot.SetActive(!IsEditMode);
        }
        public void OnCancelEdit()
        {
            IsEditMode = false;
            EditModeRoot.SetActive(IsEditMode);
            ViewModeRoot.SetActive(!IsEditMode);
        }
    }
}
