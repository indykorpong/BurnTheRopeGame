using UnityEngine;

namespace BurnTheRope.Geometry
{
    public class Line
    {
        public Vector3 start;
        public Vector3 end;

        public (int p0, int p1) pointIndices;
        public int lineIndex;
        
        public bool isVisible;

        // TODO: Remove this constructor and use the one below instead
        public Line(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
            isVisible = true;
        }

        public Line(int lineIndex, int p0, int p1)
        {
            this.lineIndex = lineIndex;
            pointIndices.p0 = p0;
            pointIndices.p1 = p1;
            isVisible = true;
        }
    }
}