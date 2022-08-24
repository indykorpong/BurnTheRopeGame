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
        private Vector3? _burnPoint;
        private int _lineIndex;

        private const float MOUSE_CURSOR_RADIUS = 0.15f;
        private const float DISTANCE_ERROR = MOUSE_CURSOR_RADIUS;

        private void Awake()
        {
            points = new List<Point>();
            
            for (int i = 0; i < pointsTransform.childCount; i++)
            {
                Point point = new Point(i, pointsTransform.GetChild(i).position);
                points.Add(point);
            }
            
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].lineIndex = i;
                lines[i].isVisible = true;

                int p0 = lines[i].pointIndex0;
                int p1 = lines[i].pointIndex1;
                points[p0].lineIndices.Add(i);
                points[p1].lineIndices.Add(i);
            }
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
                if (lineIndex == -1 || distance > DISTANCE_ERROR)
                {
                    _burnPoint = null;
                    _lineIndex = -1;
                }
                else
                {
                    _burnPoint = pointOnPath;
                    _lineIndex = lineIndex;
                    AddPointOnPath(_burnPoint.Value, _lineIndex);
                }
            }

            BurnLine();
        }

        private void DrawPath(Camera cam)
        {
            using (Draw.Command(cam))
            {
                foreach (Line line in lines)
                {
                    Draw.Line(points[line.pointIndex0].position, points[line.pointIndex1].position, 0.1f, Color.white);
                }
                
                foreach (Point point in points)
                {
                    Draw.Disc(point.position, Quaternion.identity,0.15f, DiscColors.Flat(Color.blue));
                }

                Draw.Disc(_mousePos, Quaternion.identity, MOUSE_CURSOR_RADIUS, DiscColors.Flat(Color.red));

                if (_burnPoint != null)
                {
                    Draw.Disc(_burnPoint.Value, Quaternion.identity, MOUSE_CURSOR_RADIUS, DiscColors.Flat(Color.yellow));
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
                Vector3 p1 = points[line.pointIndex0].position;
                Vector3 p2 = points[line.pointIndex1].position;

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

        private const float BURN_SPEED = 3f;
        
        private void BurnLine()
        {
            if (_burnPoint == null) return;
            
            _burnPoint = Vector3.MoveTowards(_burnPoint.Value, points[lines[_lineIndex].pointIndex0].position, BURN_SPEED * Time.deltaTime);
            points[lines[_lineIndex].pointIndex1].position = _burnPoint.Value;
        }
        
        private void AddPointOnPath(Vector3 clickPoint, int lineIndex)
        {
            Line[] newLines = new Line[lines.Count + 1];
            lines.CopyTo(newLines);

            Point[] newPoints = new Point[points.Count + 2];
            points.CopyTo(newPoints);

            newPoints[points.Count] = new Point(points.Count, clickPoint);
            newPoints[points.Count].AddLineIndex(lineIndex);
            
            newPoints[points.Count + 1] = new Point(points.Count + 1, clickPoint);
            newPoints[points.Count + 1].AddLineIndex(lines.Count);

            int cachedLineIndex = lines[lineIndex].pointIndex0;
            lines[lineIndex].pointIndex0 = points.Count;
            lines[lineIndex].pointIndex1 = lines[lineIndex].pointIndex1;

            newLines[lines.Count] = new Line(lines.Count, points.Count + 1, cachedLineIndex);

            lines = newLines.ToList();
            points = newPoints.ToList();
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