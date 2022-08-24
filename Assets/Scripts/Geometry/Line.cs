using System;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    [Serializable]
    public class Line
    {
        public int lineIndex;
        public int pointIndex0;
        public int pointIndex1;
        
        public bool isVisible;

        public Line(int lineIndex, int pointIndex0, int pointIndex1)
        {
            this.lineIndex = lineIndex;
            this.pointIndex0 = pointIndex0;
            this.pointIndex1 = pointIndex1;
            isVisible = true;
        }
    }
}