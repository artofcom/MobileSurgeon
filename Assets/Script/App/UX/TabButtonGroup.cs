using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.UX
{
    public class TabButtonGroup : MonoBehaviour
    {
        [SerializeField] List<GameObject> ButtonTurnedOn;
        [SerializeField] List<GameObject> ButtonClickable;

        void Start()
        {
            System.Diagnostics.Debug.Assert(ButtonTurnedOn.Count == ButtonClickable.Count);
        }

        public void TurnOnTabButton(int index)
        {
            for (int k = 0; k < ButtonTurnedOn.Count; ++k)
            {
                ButtonTurnedOn[k].SetActive(k == index);
                ButtonClickable[k].SetActive(k != index);
            }
        }
    }
}
