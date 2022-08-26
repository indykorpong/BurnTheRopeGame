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
                lines[i].isBurning = false;

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
                }
            }

            foreach (var kvp in _burnPointIndexToLineIndexDict)
            {
                lines[kvp.Value].isBurning = true;
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
                    Draw.Line(points[line.p0].position, points[line.p1].position, 0.1f, Color.white);
                }
                
                foreach (Point point in points)
                {
                    Draw.Disc(point.position, Quaternion.identity,0.15f, DiscColors.Flat(Color.blue));
                    Draw.Text(point.position, Quaternion.identity, point.pointIndex.ToString(), Color.green);
                }

                Draw.Disc(_mousePos, Quaternion.identity, MOUSE_CURSOR_RADIUS, DiscColors.Flat(Color.red));

                foreach (var kvp in _burnPointIndexToLineIndexDict)
                {
                    Draw.Disc(points[kvp.Key].position, Quaternion.identity, MOUSE_CURSOR_RADIUS, DiscColors.Flat(Color.magenta));
                }
                
                foreach (Point point in points)
                {
                    Draw.Text(point.position, Quaternion.identity, point.pointIndex.ToString(), Color.green);
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
            if (!lines[lineIndex].isBurning) return;
            
            int p0 = lines[lineIndex].p0;
            int p1 = lines[lineIndex].p1;
            
            int currentPointIndex;
            int nextPointIndex;

            if (p0 == burnPointIndex)
            {
                currentPointIndex = p0;
                nextPointIndex = p1;
            }
            else
            {
                currentPointIndex = p1;
                nextPointIndex = p0;
            }
            
            Debug.Log(burnPointIndex + " " + currentPointIndex + " " + nextPointIndex);

            if (Vector3.Distance(points[burnPointIndex].position, points[nextPointIndex].position) < DISTANCE_THRESHOLD)
            {
                points[burnPointIndex].position = points[nextPointIndex].position;
                points[currentPointIndex].position = points[nextPointIndex].position;

                lines[lineIndex].isBurning = false;
                
                if (points[nextPointIndex].lineIndices.Count > 0)
                {
                    foreach (int nextLineIndex in points[nextPointIndex].lineIndices)
                    {
                        if (nextLineIndex != lineIndex && !lines[nextLineIndex].isBurning &&
                            !_addBurnPointIndexToLineIndexDict.ContainsKey(nextPointIndex))
                        {
                            _addBurnPointIndexToLineIndexDict.Add(nextPointIndex, nextLineIndex);
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
            lines[lineIndex].p1 = lines[lineIndex].p1;

            newLines[lines.Count] = new Line(lines.Count, points.Count + 1, cachedLineIndex);

            lines = newLines.ToList();
            points = newPoints.ToList();

            return (p0, l0, p1, l1);
        }

        private void OnDrawGizmos()
        {
            Vector3 offset = new Vector3(0.01f, 0.01f, 0);
            int a = 0;
            Handles.color = Color.red;
            foreach (Point point in points)
            {
                Handles.Label(point.position + a * offset, point.pointIndex.ToString());
                a++;
            }
        }
    }
}