using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Core.Tween
{
    public class Mover : EaseFuncLib
    {
        // Trigger  -----------------------------------
        //
        public void Trigger(Vector3 vStart, Vector3 vEnd, float duration, Action finAction = null)
        {
            UpdateEaseFunction();

            StartCoroutine(coTween(vStart, vEnd, duration, finAction));
        }

        public void TriggerWithEase(DurationEase easeType, Vector3 vStart, Vector3 vEnd, float duration, Action finAction)
        {
            EaseType = easeType;

            Trigger(vStart, vEnd, duration, finAction);
        }





        // Member func  -----------------------------------
        //
        IEnumerator coTween(Vector3 vStart, Vector3 vEnd, float duration, Action finAction)
        {
            transform.localPosition = vStart;

            float fStartT = Time.time;
            while (Time.time < fStartT + duration)
            {
                //transform.localScale = Vector3.Lerp(vStart, vTo, Mathf.Clamp01((Time.time - fStartT) / duration));

                float timeDelta = durationEaseFunc(Time.time - fStartT, duration);
                transform.localPosition = Vector3.LerpUnclamped(vStart, vEnd, timeDelta);

                yield return null;
            }
            transform.localPosition = vEnd;

            if (finAction != null)
                finAction.Invoke();
        }

    }

}