using System;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    [Serializable]
    public class Line
    {
        public int lineIndex;
        public int p0;
        public int p1;
        
        public bool isBurning;

        public Line(int lineIndex, int p0, int p1)
        {
            this.lineIndex = lineIndex;
            this.p0 = p0;
            this.p1 = p1;
            isBurning = false;
        }
    }
}