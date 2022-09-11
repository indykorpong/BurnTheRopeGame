using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEditor;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    [Serializable]
    public class Path : MonoBehaviour
    {
        [SerializeField] private List<Line> lines;
        [SerializeField] private List<Point> points;
        [SerializeField] private Transform pointsTransform;

        private Camera _camera;
        private Vector3 _mousePos;
        
        private Dictionary<int, int> _burnPointIndexToLineIndexDict;
        private Dictionary<int, int> _addBurnPointIndexToLineIndexDict;

        private const float MOUSE_CURSOR_RADIUS = 0.15f;
        private const float DISTANCE_ERROR = MOUSE_CURSOR_RADIUS;

        private void Awake()
        {
            points = new List<Point>();
            
            for (int i = 0; i < pointsTransform.childCount; i++)
            {
                if (pointsTransform.GetChild(i).gameObject.activeSelf)
                {
                    Point point = new Point(i, pointsTransform.GetChild(i).position);
                    points.Add(point);
                }
            }
            
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].lineIndex = i;
                lines[i].lineStatus = LineStatus.NotBurned;
                lines[i].formerLineIndex = -1;

                int p0 = lines[i].p0;
                int p1 = lines[i].p1;
                points[p0].lineIndices.Add(i);
                points[p1].lineIndices.Add(i);
            }

            _burnPointIndexToLineIndexDict = new Dictionary<int, int>();
            _addBurnPointIndexToLineIndexDict = new Dictionary<int, int>();
        }

        private void Start()
        {
            _camera = FindObjectOfType<Camera>();
            Camera.onPreRender += DrawPath;
        }

        private void Update()
        {
            _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePos.z = 0;

            if (Input.GetMouseButtonDown(0))
            {
                (Vector3 pointOnPath, int lineIndex, float distance) = GetNearestPointOnPath(_mousePos);
                if (lineIndex != -1 && distance <= DISTANCE_ERROR)
                {
                    (int p0, int l0, int p1, int l1) = AddPointsOnPath(pointOnPath, lineIndex);
                    _burnPointIndexToLineIndexDict.Add(p0, l0);
                    _burnPointIndexToLineIndexDict.Add(p1, l1);

                    lines[l0].formerLineIndex = lineIndex;
                    lines[l1].formerLineIndex = lineIndex;
                }
            }

            foreach (var kvp in _burnPointIndexToLineIndexDict)
            {
                BurnLine(kvp.Key, kvp.Value);
            }

            foreach (var kvp in _addBurnPointIndexToLineIndexDict)
            {
                if (_burnPointIndexToLineIndexDict.ContainsKey(kvp.Key)) continue;
                _burnPointIndexToLineIndexDict.Add(kvp.Key, kvp.Value);
            }

            _addBurnPointIndexToLineIndexDict.Clear();
        }

        private void DrawPath(Camera cam)
        {
            using (Draw.Command(cam))
            {
                foreach (Line line in lines)
                {
                    if (line.lineStatus == LineStatus.IsBurned) continue;
                    DrawLine(line);
                    DrawPoint(line.p0, Color.blue);
                    DrawPoint(line.p1, Color.blue);
                }

                Draw.Disc(_mousePos, Quaternion.identity, MOUSE_CURSOR_RADIUS, DiscColors.Flat(Color.red));

                foreach (var kvp in _burnPointIndexToLineIndexDict)
                {
                    if (lines[kvp.Value].lineStatus == LineStatus.IsBurned) continue;
                    DrawPoint(kvp.Key, Color.magenta);
                }
            }
        }
        
        private (Vector3, int, float) GetNearestPointOnPath(Vector3 mousePos)
        {
            float nearestDistance = float.MaxValue;
            Vector3 nearestPoint = Vector3.zero;
            int nearestIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                Line line = lines[i];
                Vector3 p1 = points[line.p0].position;
                Vector3 p2 = points[line.p1].position;

                float t = InverseLerp(p1, p2, mousePos);
                if (t < 0 || t > 1) continue;
                
                float x1 = p1.x;
                float y1 = p1.y;
                float x2 = p2.x;
                float y2 = p2.y;

                float xi = x1 + (x2 - x1) * t;
                float yi = y1 + (y2 - y1) * t;
                float zi = mousePos.z;
                Vector3 point = new Vector3(xi, yi, zi);

                float distance = Vector3.Distance(mousePos, point);
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

        private Vector3 Lerp(Vector3 start, Vector3 end, float t)
        {
            return start + (end - start) * t;
        }

        private const float BURN_SPEED = 1f;
        private const float DISTANCE_THRESHOLD = (float)1e-6;
        
        private void BurnLine(int burnPointIndex, int lineIndex)
        {
            if (lines[lineIndex].lineStatus == LineStatus.IsBurned) return;

            lines[lineIndex].lineStatus = LineStatus.IsBurning;
            
            int p0 = lines[lineIndex].p0;
            int p1 = lines[lineIndex].p1;
            
            int nextPointIndex;

            if (p0 == burnPointIndex)
            {
                nextPointIndex = p1;
            }
            else
            {
                nextPointIndex = p0;
            }
            
            if (Vector3.Distance(points[burnPointIndex].position, points[nextPointIndex].position) < DISTANCE_THRESHOLD)
            {
                points[burnPointIndex].position = points[nextPointIndex].position;
                lines[lineIndex].lineStatus = LineStatus.IsBurned;
                
                if (points[nextPointIndex].lineIndices.Count > 0)
                {
                    foreach (int nextLineIndex in points[nextPointIndex].lineIndices)
                    {
                        if (nextLineIndex != lineIndex && lines[nextLineIndex].lineStatus != LineStatus.IsBurned)
                        {
                            if (lines[nextLineIndex].formerLineIndex != -1 && 
                                lines.FindIndex(line => line.formerLineIndex == lines[nextLineIndex].formerLineIndex) != -1) continue;
                            if (_addBurnPointIndexToLineIndexDict.ContainsKey(nextPointIndex))
                            {
                                int newBurnPointIndex = AddBurnPointOnPath(nextPointIndex, nextLineIndex);
                                _addBurnPointIndexToLineIndexDict.Add(newBurnPointIndex, nextLineIndex);
                            }
                            else
                            {
                                _addBurnPointIndexToLineIndexDict.Add(nextPointIndex, nextLineIndex);
                            }
                        }
                    }
                }
            }

            points[burnPointIndex].position = Vector3.MoveTowards(
                points[burnPointIndex].position,
                points[nextPointIndex].position,
                BURN_SPEED * Time.deltaTime);
        }
        
        private (int, int, int, int) AddPointsOnPath(Vector3 clickPoint, int lineIndex)
        {
            Line[] newLines = new Line[lines.Count + 1];
            lines.CopyTo(newLines);

            Point[] newPoints = new Point[points.Count + 2];
            points.CopyTo(newPoints);

            newPoints[points.Count] = new Point(points.Count, clickPoint);
            newPoints[points.Count].AddLineIndex(lineIndex);

            int p0 = points.Count;
            int l0 = lineIndex;
            
            newPoints[points.Count + 1] = new Point(points.Count + 1, clickPoint);
            newPoints[points.Count + 1].AddLineIndex(lines.Count);

            int p1 = points.Count + 1;
            int l1 = lines.Count;

            int cachedLineIndex = lines[lineIndex].p0;
            lines[lineIndex].p0 = points.Count;

            newLines[lines.Count] = new Line(lines.Count, points.Count + 1, cachedLineIndex);

            lines = newLines.ToList();
            points = newPoints.ToList();

            return (p0, l0, p1, l1);
        }

        private int AddBurnPointOnPath(int nextPointIndex, int nextLineIndex)
        {
            Point[] newPoints = new Point[points.Count + 1];
            points.CopyTo(newPoints);

            int newPointIndex = points.Count;
            newPoints[newPointIndex] = new Point(newPointIndex, points[nextPointIndex].position);
            newPoints[newPointIndex].AddLineIndex(nextLineIndex);

            if (lines[nextLineIndex].p0 == nextPointIndex)
            {
                lines[nextLineIndex].p0 = newPointIndex;
            }
            else if(lines[nextLineIndex].p1 == nextPointIndex)
            {
                lines[nextLineIndex].p1 = newPointIndex;
            }

            points = newPoints.ToList();

            return newPointIndex;
        }

        private void DrawPoint(int pointIndex, Color pointColor)
        {
            Draw.Disc(points[pointIndex].position, Quaternion.identity,0.15f, DiscColors.Flat(pointColor));
            Draw.Text(points[pointIndex].position, Quaternion.identity, pointIndex.ToString(), Color.green); 
        }

        private void DrawLine(Line line)
        {
            Draw.Line(points[line.p0].position, points[line.p1].position, 0.1f, Color.white); 
        }
    }
}