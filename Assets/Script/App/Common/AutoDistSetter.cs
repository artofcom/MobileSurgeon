using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace App.Common
{
    public class AutoDistSetter : MonoBehaviour
    {
        [SerializeField] Transform Target1;
        [SerializeField] Transform Target2;
        [SerializeField] Volume PostVolume;

        [SerializeField] float UpdateTick = 0.2f;

        UnityEngine.Rendering.Universal.DepthOfField mDepthOfField;
        UnityEngine.Rendering.MinFloatParameter mMinFloat = null;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(coUpdate());
        }

        IEnumerator coUpdate()
        {
            if (PostVolume == null)
                yield break;

            var waitForSecond = new WaitForSeconds(UpdateTick);

            while (true)
            {
                yield return waitForSecond;

                VolumeProfile volumeProfile = PostVolume.profile;

                if (volumeProfile.TryGet(out mDepthOfField) == false)
                    continue;

                if (!mDepthOfField.active)
                    yield break;

                mMinFloat = mDepthOfField.focusDistance;
                mMinFloat.value = Vector3.Distance(Target1.position, Target2.position);
                mDepthOfField.focusDistance = mMinFloat;
            }
        }
    }
}
