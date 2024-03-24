using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;

namespace App.MVCS
{
    public class NoteItemView : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Button BtnNote;

        float mTimeRate;
        // Start is called before the first frame update
        void Start()
        {
            BtnNote.onClick.AddListener(this.OnClick);
        }

        public void Init(float fTimeRate)
        {
            mTimeRate = fTimeRate;
        }

        // Update is called once per frame
        public void OnClick()
        {
            EventSystem.DispatchEvent("OnNoteItemClicked", (object)mTimeRate);
        }
    }
}