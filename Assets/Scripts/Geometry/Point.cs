using System.Collections.Generic;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    public class Point
    {
        public int pointIndex;
        public Vector3 position;
        public List<int> lineIndices;

        public Point(int pointIndex, Vector3 position)
        {
            this.pointIndex = pointIndex;
            this.position = position;

            lineIndices = new List<int>();
        }

        public void AddLineIndex(int lineIndex)
        {
            lineIndices.Add(lineIndex);
        }
    }
}