using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Events;
using TMPro;
using Unity.Services.Authentication;

namespace App.MVCS
{
    public class OptionTabView : MonoBehaviour
    {
        [SerializeField] TMP_Text txtCacheSize;
        [SerializeField] TMP_Text txtVersion;
        [SerializeField] TMP_Text txtId, txtPlayerName;
        [SerializeField] TMP_Dropdown DDQualityMode;

        public void OnClickClearCache()
        {
            EventSystem.DispatchEvent("OptionTabView_OnClickClearCache");
        }

        public void StartView(long cacheSize)
        {
            string number = ToFileSize((double)cacheSize);
            if (txtCacheSize != null)
                txtCacheSize.text = $"Cache size : {number}";

            if (txtVersion != null)
                txtVersion.text = "Version : " + Application.version;

            if (txtPlayerName != null)
                txtPlayerName.text = string.IsNullOrEmpty(AuthenticationService.Instance.PlayerInfo.Username) ? "Name : N/A" :  "Name : " + AuthenticationService.Instance.PlayerInfo.Username;

            if (txtId != null)
                txtId.text = "Id : " + AuthenticationService.Instance.PlayerId;

            if (DDQualityMode != null)
                DDQualityMode.value = QualitySettings.GetQualityLevel();
        }



        public void OnQualityModeChanged(int index)
        {
            Debug.Log($"Quality Mode changed...{index}");
            QualitySettings.SetQualityLevel(index, true);
        }
        public void OnDebugDisplayModeChanged(int index)
        {
            EventSystem.DispatchEvent("OptionTabView_OnDebugDisplayModeChanged", (object)index);
        }
        public void OnBtnSignOutClicked()
        {
            EventSystem.DispatchEvent("OptionTabView_OnBtnSignOutClicked");
        }

        //
        // Helper Functions.
        //
        // http://www.csharphelper.com/howtos/howto_file_size_in_words.html
        //
        public static string ToFileSize(double value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB",
            "TB", "PB", "EB", "ZB", "YB"};
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Mathf.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value /
                        Mathf.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(value /
                Mathf.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        // Return the value formatted to include at most three
        // non-zero digits and at most two digits after the
        // decimal point. Examples:
        //         1
        //       123
        //        12.3
        //         1.23
        //         0.12
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }
    }

}