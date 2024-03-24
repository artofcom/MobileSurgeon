using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Common
{
    public class CullingOrigin : MonoBehaviour
    {
        public float Radius = 0.3f;

        // Start is called before the first frame update
        void Start()
        {

        }

        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
