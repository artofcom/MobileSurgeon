using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Common
{
    public class CullingTrigger : MonoBehaviour
    {
        [SerializeField] CullingOrigin CullingCenter;
        [SerializeField] Transform MovingTransform;
        [SerializeField] List<GameObject> CullingTargets = new List<GameObject>();
        [SerializeField] float CullCheckInterval = 0.1f;

        // Start is called before the first frame update
        void Start()
        {
            UnityEngine.Assertions.Assert.IsTrue(CullingCenter != null);
            UnityEngine.Assertions.Assert.IsTrue(MovingTransform != null);
            UnityEngine.Assertions.Assert.IsTrue(CullingTargets.Count > 0);

            StartCoroutine(coUpdate());
        }

        IEnumerator coUpdate()
        {
            WaitForSeconds waitTime = new WaitForSeconds(CullCheckInterval);

            while (true)
            {
                yield return waitTime;

                float fDist = Vector3.Distance(CullingCenter.transform.position, MovingTransform.position);
                bool visible = fDist <= CullingCenter.Radius;

                for (int q = 0; q < CullingTargets.Count; ++q)
                    CullingTargets[q].SetActive(visible);
            }
        }

    }
}