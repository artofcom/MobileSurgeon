using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace App.Common
{

    public class VolumeQualitySelector : MonoBehaviour
    {
        [SerializeField] Volume PostVolume;

        [SerializeField] VolumeProfile ProfileHigh;
        [SerializeField] VolumeProfile ProfileMedium;
        [SerializeField] VolumeProfile ProfileLow;

        // Start is called before the first frame update
        void Start()
        {
            int level = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, 2);
            switch (level)
            {
                default:
                case 0:
                    PostVolume.profile = ProfileLow;
                    break;
                case 1:
                    PostVolume.profile = ProfileMedium;
                    break;
                case 2:
                    PostVolume.profile = ProfileHigh;
                    break;
            }
            Debug.Log($"Volum Quality was set to {level}");
        }
    }
}