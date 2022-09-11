using System;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    public enum LineStatus
    {
        NotBurned,
        IsBurning,
        IsBurned
    }
    [Serializable]
    public class Line
    {
        public int lineIndex;
        public int formerLineIndex;
        public int p0;
        public int p1;
        
        public LineStatus lineStatus;

        public Line(int lineIndex, int p0, int p1)
        {
            this.lineIndex = lineIndex;
            this.p0 = p0;
            this.p1 = p1;
            formerLineIndex = -1;
            lineStatus = LineStatus.NotBurned;
        }
    }
}