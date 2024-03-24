using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Common
{
    public class Util
    {
        public long TimeToFrame(double time, int fps = 30)
        {
            return (long)(time * ((double)fps));
        }

        public double FrameToTime(long frame, int fps = 30)
        {
            return (frame / (double)fps);
        }

    }
}
