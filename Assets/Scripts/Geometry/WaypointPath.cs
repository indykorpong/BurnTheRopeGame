using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    // TODO: Migrate from using this to using `Path` class
    public class WaypointPath
    {
        private readonly List<Vector3> _waypoints;
        private List<Line> _lines;          // lines to burn
        private List<Line> _cachedLines;    // invisible lines that are not burned and are copied from `_lines`

        // Construct waypoint path from a list of continuous points
        // as in the line you will draw contains this list of continuous points.
        public WaypointPath(List<Vector3> positions)
        {
            _waypoints = positions;
            _lines = new List<Line>();
            _cachedLines = new List<Line>();

            CalculateLines();
        }

        private void CalculateLines()
        {
            _lines.Clear();
            _cachedLines.Clear();
            
            for (int i = 0; i < _waypoints.Count - 1; i++)
            {
                Line line = new Line(_waypoints[i], _waypoints[i + 1]);
                _lines.Add(line);
                
                Line cachedLine = new Line(_waypoints[i], _waypoints[i + 1]);
                _cachedLines.Add(cachedLine);
            }
        }

        public void DrawLines(Camera cam)
        {
            using (Draw.Command(cam))
            {
                foreach (Line line in _lines)
                {
                    if (!line.isVisible) continue;
                    Draw.Line(line.start, line.end, 0.1f, Color.black);
                }
            }
        }

        private void AddPointOnPath(ref List<Line> lines, Vector3 point, int lineIndex)
        {
            Line[] newLineArray = new Line[lines.Count];
            lines.CopyTo(newLineArray);

            List<Line> newLineList = new List<Line>();
            newLineList.AddRange(lines.GetRange(0, lineIndex));
            newLineList.Add(new Line(lines[lineIndex].start, point));
            newLineList.Add(new Line(point, lines[lineIndex].end));
            newLineList.AddRange(lines.GetRange(lineIndex + 1, lines.Count - lineIndex - 1));

            lines = newLineList;
        }

        private void RemovePointAtIndex(ref List<Line> lines, int index)
        {
            Line[] newLineArray = new Line[lines.Count];
            lines.CopyTo(newLineArray);

            List<Line> newLineList = new List<Line>();
            newLineList.RemoveAt(index);

            lines = newLineList;
        }

        private Vector3? _clickPoint;

        private int _leftCurrentIndex;
        private Vector3 _leftBurnPoint;

        private int _rightCurrentIndex;
        private int _rightNextIndex;
        private Vector3 _rightBurnPoint;

        private const float DISTANCE_ERROR = LineDrawer.CLICK_POINT_RADIUS;
        private const float BURN_SPEED = 3f;
        private const float LEFT_THRESHOLD = 0.001f;
        private const float RIGHT_THRESHOLD = 0.999f;
        private const float DIST_THRESHOLD = 0.001f;

        private bool _leftFinishedBurning;
        private bool _rightFinishedBurning;

        public void SetBurnPoint(Vector3 mousePos)
        {
            _clickPoint = mousePos;
            
            (Vector3 burnPoint, int lineIndex, float distance) = GetNearestPointOnPath(_clickPoint.Value);
            if (lineIndex == -1 || distance > DISTANCE_ERROR)
            {
                _clickPoint = null;
                return;
            }

            _leftBurnPoint = burnPoint;
            _rightBurnPoint = burnPoint;

            _leftFinishedBurning = false;
            _rightFinishedBurning = false;

            AddPointOnPath(ref _lines, burnPoint, lineIndex);
            AddPointOnPath(ref _cachedLines, burnPoint, lineIndex);

            _leftCurrentIndex = lineIndex;
            _rightCurrentIndex = lineIndex + 1;
        }

        /*
         * Returns true if the click point is near the lines enough and the lines have finished burning.
         * Returns false if the click point is too far from the lines.
         */
        public bool BurnLines()
        {
            if (_clickPoint == null) return false;
            
            // Left side
            if (Vector3.Distance(_leftBurnPoint, _lines[0].start) < DIST_THRESHOLD)
            {
                _leftFinishedBurning = true;
            }

            if (!_leftFinishedBurning)
            {
                _leftBurnPoint = Vector3.MoveTowards(_leftBurnPoint, _lines[_leftCurrentIndex].start,
                    BURN_SPEED * Time.deltaTime);
                _lines[_leftCurrentIndex].end = _leftBurnPoint;

                /*
                 * If the current burn point reached the next waypoint,
                 * then set the position of the burn point to the position of the next waypoint,
                 * set isVisible of the current line to false,
                 * and decrement the current waypoint index.
                 */
                if (InverseLerp(_cachedLines[_leftCurrentIndex].start, _cachedLines[_leftCurrentIndex].end, _leftBurnPoint) <
                    LEFT_THRESHOLD)
                {
                    _leftBurnPoint = _lines[_leftCurrentIndex].start;
                    _lines[_leftCurrentIndex].isVisible = false;

                    _leftCurrentIndex--;
                    if (_leftCurrentIndex >= 0)
                    {
                        _leftBurnPoint = _lines[_leftCurrentIndex].end;
                    }
                }
            }

            // Right side
            if (Vector3.Distance(_rightBurnPoint, _lines[^1].end) < DIST_THRESHOLD)
            {
                _rightFinishedBurning = true;
            }

            if (!_rightFinishedBurning)
            {
                _rightBurnPoint = Vector3.MoveTowards(_rightBurnPoint, _lines[_rightCurrentIndex].end,
                    BURN_SPEED * Time.deltaTime);
                _lines[_rightCurrentIndex].start = _rightBurnPoint;

                if (InverseLerp(_cachedLines[_rightCurrentIndex].start, _cachedLines[_rightCurrentIndex].end, _rightBurnPoint) >
                    RIGHT_THRESHOLD)
                {
                    _rightBurnPoint = _lines[_rightCurrentIndex].end;
                    _lines[_rightCurrentIndex].isVisible = false;

                    _rightCurrentIndex++;
                    if (_rightCurrentIndex < _lines.Count)
                    {
                        _rightBurnPoint = _lines[_rightCurrentIndex].start;
                    }
                }
            }

            return _leftFinishedBurning && _rightFinishedBurning;
        }

        private (Vector3, int, float) GetNearestPointOnPath(Vector3 clickPoint)
        {
            float nearestDistance = float.MaxValue;
            Vector3 nearestPoint = Vector3.zero;
            int nearestIndex = -1;

            for (int i = 0; i < _lines.Count; i++)
            {
                Line line = _lines[i];

                float t = InverseLerp(line.start, line.end, clickPoint);
                if (t < 0 || t > 1) continue;
                
                float x1 = line.start.x;
                float y1 = line.start.y;
                float x2 = line.end.x;
                float y2 = line.end.y;

                float m = (y2 - y1) / (x2 - x1);
                float c = y1 - m * x1;

                float xi = clickPoint.x;
                float yi = m * xi + c;
                float zi = clickPoint.z;
                Vector3 point = new Vector3(xi, yi, zi);

                float distance = Vector3.Distance(clickPoint, point);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPoint = point;
                    nearestIndex = i;
                }
            }

            return (nearestPoint, nearestIndex, nearestDistance);
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