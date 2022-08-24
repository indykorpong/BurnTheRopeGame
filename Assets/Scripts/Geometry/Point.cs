using System;
using System.Collections.Generic;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    [Serializable]
    public class Point
    {
        public int pointIndex;
        public Vector3 position;
        public List<int> lineIndices = new();

        public Point(int pointIndex, Vector3 position)
        {
            this.pointIndex = pointIndex;
            this.position = position;
        }

        public void AddLineIndex(int lineIndex)
        {
            lineIndices.Add(lineIndex);
        }
    }
}