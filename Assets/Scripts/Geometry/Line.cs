using UnityEngine;

namespace BurnTheRope.Geometry
{
    public class Line
    {
        public Vector3 start;
        public Vector3 end;
        public bool isVisible;

        public Line(Vector3 start, Vector3 end)
        {
            this.start = start;
            this.end = end;
            isVisible = true;
        }
    }
}