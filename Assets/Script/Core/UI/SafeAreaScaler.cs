using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Core.UI
{
    public class SafeAreaScaler : MonoBehaviour
    {
        [SerializeField] Vector2 TargetResolution;
        [SerializeField] Transform[] TargetTransforms;

        Rect LastSafeArea = new Rect(0, 0, 0, 0);
        List<Vector3> vOrgScales = new List<Vector3>();

        private void Start()
        {
            if (TargetTransforms != null)
            {
                for (int q = 0; q < TargetTransforms.Length; ++q)
                    vOrgScales.Add(TargetTransforms[q].localScale);
            }
        }

        private void Update()
        {
            Rect safeArea = Screen.safeArea;
            if (safeArea != LastSafeArea)
            {
                LastSafeArea = safeArea;
                StartCoroutine(coRunWithDelay(.01f, () => RescaleCanvas()));
            }
        }


        // Give some gap to seek the real values.
        IEnumerator coRunWithDelay(float delay, System.Action func)
        {
            if (delay > .0f)
                yield return new WaitForSeconds(delay);

            func.Invoke();
        }

        // Rescale Transform.
        void RescaleCanvas()
        {
            if (TargetTransforms != null)
            {
                float scale = ((float)Screen.safeArea.height) / ((float)Screen.height);

                for (int k = 0; k < TargetTransforms.Length; ++k)
                    TargetTransforms[k].localScale = vOrgScales[k] * scale;

                Debug.Log($"===== Res has been re-scaled.. ===={Screen.safeArea.height}, {Screen.height} ");
            }
        }
    }
}