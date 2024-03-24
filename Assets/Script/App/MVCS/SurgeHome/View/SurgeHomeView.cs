using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using App.UX;

namespace App.MVCS
{
    public class SurgeHomeView : AView
    {
        public HomeTabView HomeTabView;
        public SpecialtyTabView SpecialtyTabView;
        public CPTTabView CPTTabView;
        public OptionTabView OptionTabView;
        [SerializeField] TabButtonGroup TabButtons;

        // Start is called before the first frame update
        void Start()
        { }

        private void OnEnable()
        {
            EventSystem.DispatchEvent("SurgeHomeView_OnEnable");
        }

        public void InitView()
        {
            OnHomeTabBtnClicked();
        }

        public void OnHomeTabBtnClicked()
        {
            HomeTabView.gameObject.SetActive(true);
            SpecialtyTabView.gameObject.SetActive(false);
            CPTTabView.gameObject.SetActive(false);
            OptionTabView.gameObject.SetActive(false);
            TabButtons.TurnOnTabButton(0);
        }
        public void OnSpecialtyTabBtnClicked()
        {
            HomeTabView.gameObject.SetActive(false);
            SpecialtyTabView.gameObject.SetActive(true);
            CPTTabView.gameObject.SetActive(false);
            OptionTabView.gameObject.SetActive(false);
            TabButtons.TurnOnTabButton(1);
        }
        public void OnCPTTabBtnClicked()
        {
            HomeTabView.gameObject.SetActive(false);
            SpecialtyTabView.gameObject.SetActive(false);
            CPTTabView.gameObject.SetActive(true);
            OptionTabView.gameObject.SetActive(false);
            TabButtons.TurnOnTabButton(2);
        }
        public void OnOptionTabBtnClicked()
        {
            HomeTabView.gameObject.SetActive(false);
            SpecialtyTabView.gameObject.SetActive(false);
            CPTTabView.gameObject.SetActive(false);
            OptionTabView.gameObject.SetActive(true);
            TabButtons.TurnOnTabButton(3);

            EventSystem.DispatchEvent("SurgeHomeView_OnOptionTabClicked");
        }
    }
}