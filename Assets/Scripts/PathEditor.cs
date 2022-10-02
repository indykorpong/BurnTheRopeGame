using System;
using System.Collections.Generic;
using BurnTheRope.Geometry;
using Shapes;
using UnityEngine;
using UnityEngine.EventSystems;
using Line = BurnTheRope.Geometry.Line;

namespace BurnTheRope
{
    public class PathEditor : MonoBehaviour
    {
        public Color extraPointColor;
        public Color extraLineColor;
        
        private Path _path;
        private Camera _camera;
        private Vector3 _mousePos;

        private List<Line> _extraLines;
        private List<Point> _extraPoints;

        private int _leftClickCount = -1;

        private void Start()
        {
            _path = FindObjectOfType<Path>();
            _camera = FindObjectOfType<Camera>();

            _extraLines = new List<Line>();
            _extraPoints = new List<Point>();
            
            Camera.onPreRender += DrawExtraPath;
        }

        private void Update()
        {
            if (GameModeButton.gameMode != GameMode.Edit) return;

            _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePos.z = -0.001f;

            if (EventSystem.current.IsPointerOverGameObject()) return;

            /*
             * If the clicked point is near the existing point, draw the line from that point to the cursor point.
             * Otherwise, draw the line from the clicked point to the cursor point.
             * https://i.imgur.com/fE9VbpV.png
             */
            
            if (Input.GetMouseButtonDown(0))
            {
                _leftClickCount++;
                
                Point nearestPoint = _path.GetNearestPointOnPath(_mousePos);
                if (nearestPoint.pointIndex != -1)
                {
                    Point pointOnExistingPath = _path.Points[nearestPoint.pointIndex];
                    pointOnExistingPath.color = extraPointColor;
                    pointOnExistingPath.isExtraPointTheSamePosition = true;
                    
                    _extraPoints.Add(pointOnExistingPath);
                }
                else
                {
                    Point extraPoint = new Point(
                        _extraPoints.Count,
                        nearestPoint.position,
                        new List<int> { _extraLines.Count },
                        extraPointColor);
                    _extraPoints.Add(extraPoint);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                _leftClickCount = -1;
            }

            if (_leftClickCount == 0)
            {
                // TODO: edit the following line to update the position of _extraPoints[^1] instead of creating it every frame
                Point drawingPoint = new Point(_extraPoints.Count, _mousePos);
                drawingPoint.AddLineIndex(_extraLines.Count);

                _extraPoints.Add(drawingPoint);

                // TODO: edit the following lines not to create a new line every frame
                Line drawingLine = new Line(_extraLines.Count, _extraPoints[^2].pointIndex, _extraPoints[^1].pointIndex);
                _extraLines.Add(drawingLine);
            }
        }

        private void DrawExtraPath(Camera cam)
        {
            using (Draw.Command(cam))
            {
                foreach (Line line in _extraLines)
                {
                    Draw.Line(_extraPoints[line.p0].position, _extraPoints[line.p1].position, 0.1f, extraLineColor);
                }
                
                foreach (Point point in _extraPoints)
                {
                    Draw.Disc(point.position, Quaternion.identity, 0.15f, DiscColors.Flat(extraPointColor));
                }

                if (GameModeButton.gameMode == GameMode.Edit)
                {
                    Draw.Disc(_mousePos, Quaternion.identity, 0.15f, DiscColors.Flat(_path.cursorColor));
                }
            }
        }
    }
}