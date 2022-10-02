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
        public List<int> lineIndices;
        public Color color;

        public bool isExtraPointTheSamePosition;

        public Point(int pointIndex, Vector3 position)
        {
            this.pointIndex = pointIndex;
            this.position = position;
            lineIndices = new List<int>();
            color = Color.black;
            isExtraPointTheSamePosition = false;
        }

        public Point(int pointIndex, Vector3 position, List<int> lineIndices)
        {
            this.pointIndex = pointIndex;
            this.position = position;
            this.lineIndices = lineIndices;
            color = Color.black;
            isExtraPointTheSamePosition = false;
        }

        public Point(int pointIndex, Vector3 position, List<int> lineIndices, Color color)
        {
            this.pointIndex = pointIndex;
            this.position = position;
            this.lineIndices = lineIndices;
            this.color = color;
            isExtraPointTheSamePosition = false;
        }

        public void AddLineIndex(int lineIndex)
        {
            lineIndices.Add(lineIndex);
        }
    }
}