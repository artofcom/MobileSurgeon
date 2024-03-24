using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Tween
{
    public class Scaler : EaseFuncLib
    {
        // Trigger  -----------------------------------
        //
        public void Trigger(float startScale, float endScale, float duration, Action finAction = null)
        {
            UpdateEaseFunction();

            StartCoroutine(coScale(startScale, endScale, duration, finAction));
        }

        public void TriggerWithEase(DurationEase easeType, float startScale, float endScale, float duration, Action finAction)
        {
            EaseType = easeType;

            Trigger(startScale, endScale, duration, finAction);
        }





        // Member func  -----------------------------------
        //
        IEnumerator coScale(float startScale, float endScale, float duration, Action finAction)
        {
            Vector3 vStart = Vector3.one * startScale;
            Vector3 vTo = Vector3.one * endScale;

            transform.localScale = vStart;

            float fStartT = Time.time;
            while (Time.time < fStartT + duration)
            {
                //transform.localScale = Vector3.Lerp(vStart, vTo, Mathf.Clamp01((Time.time - fStartT) / duration));

                float timeDelta = durationEaseFunc(Time.time - fStartT, duration);
                transform.localScale = Vector3.LerpUnclamped(vStart, vTo, timeDelta);

                yield return null;
            }
            transform.localScale = vTo;

            if (finAction != null)
                finAction.Invoke();
        }

    }
}
