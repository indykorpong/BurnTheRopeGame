using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    public class WaypointPath
    {
        private readonly List<Vector3> _waypoints = new List<Vector3>();
        private List<Line> _lines = new List<Line>();

        // Construct waypoint path from a list of continuous points
        // as in the line you will draw contains this list of continuous points.
        public WaypointPath(List<Vector3> positions)
        {
            _waypoints = positions;
            
            _lines = new List<Line>();
            CalculateLines();
        }

        private void CalculateLines()
        {
            _lines.Clear();
            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                Line line = new Line(_waypoints[i], _waypoints[i + 1]);
                _lines.Add(line);
            }
        }

        public void DrawLines(Camera cam)
        {
            using (Draw.Command(cam))
            {
                foreach (Line line in _lines)
                {
                    Draw.Line(line.start, line.end, Color.black);
                }
            }
        }

        public void RemoveLine(int index)
        {
            if (_lines.Count <= index)
            {
                Debug.LogWarning("Cannot remove the line at the index: " + index);
                return;
            }
            _lines.RemoveAt(index);
        }

        private int _leftCurrentIndex;
        private int _leftNextIndex;
        private Vector3 _leftBurnPoint;

        private int _rightCurrentIndex;
        private int _rightNextIndex;
        private Vector3 _rightBurnPoint;
        
        public float burnSpeed = 5f;
        
        public void BurnLines(Vector3 clickPoint)
        {
            // Left side
            // _leftCurrentIndex = startIndex;
            // _leftNextIndex = startIndex - 1;
            // _leftBurnPoint = Vector3.MoveTowards(_leftBurnPoint, _lines[_leftCurrentIndex].start, burnSpeed * Time.deltaTime);
            // if (InverseLerp(_lines[_leftNextIndex], _points[_currentPointIndex], _burnPoint) < threshold)
            // {
            //     _burnPoints[_currentPointIndex] = _points[_nextPointIndex];
            //     _burnPoint = _points[_nextPointIndex];
            //     
            //     // DrawLine();
            //
            //     _burnPoints.RemoveAt(_currentPointIndex);
            //     
            //     _currentPointIndex--;
            //     _nextPointIndex--;
            // }
            
            // Right side
            // _rightCurrentIndex = startIndex;
            // _rightBurnPoint = Vector3.MoveTowards(_rightBurnPoint, _lines[_rightCurrentIndex].end, burnSpeed * Time.deltaTime); 
        }
        
        private float InverseLerp(Vector3 a, Vector3 b, Vector3 v)
        {
            float x = Vector3.Distance(v, a);
            float y = Vector3.Distance(b, a);
            return x / y;
        }
        
        public List<Vector3> GetWaypoints()
        {
            return _waypoints;
        }

        public List<Line> GetLines()
        {
            return _lines;
        }
    }
}