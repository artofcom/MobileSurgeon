using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Tween
{
    public class AutoRotate : MonoBehaviour
    {
        [SerializeField] public float RotateSpeedX = 0;
        [SerializeField] float RotateSpeedY = 0;
        [SerializeField] float RotateSpeedZ = 0;

        [SerializeField] bool Enabled = true;

        void Update()
        {
            if (Enabled)
            {
                float timedelta = Time.unscaledDeltaTime;
                Vector3 result = new Vector3(RotateSpeedX, RotateSpeedY, RotateSpeedZ) * timedelta;
                transform.Rotate(result);
            }
        }
    }
}
