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

        private bool _firstClickPressed;
        private bool _secondClickPressed;
        private bool _secondPointCreated;
        
        private int _firstPointIndex;
        private int _secondPointIndex;
        private int _extraLineIndex;

        private void Start()
        {
            _path = FindObjectOfType<Path>();
            _camera = FindObjectOfType<Camera>();

            _extraLines = new List<Line>();
            _extraPoints = new List<Point>();
            
            _firstClickPressed = false;
            _secondClickPressed = false;
            
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
            
            if (Input.GetMouseButtonDown(0) && _firstClickPressed && !_secondClickPressed)
            {
                _secondClickPressed = true;

                int newFirstPointIndex = _path.Points.Count;
                Point firstPoint = new Point(newFirstPointIndex, _extraPoints[_firstPointIndex].position);
                _path.Points.Add(firstPoint);

                int newSecondPointIndex = newFirstPointIndex + 1;
                Point secondPoint = new Point(newSecondPointIndex, _extraPoints[_secondPointIndex].position);
                _path.Points.Add(secondPoint);

                int newLineIndex = _path.Lines.Count;
                Line newLine = new Line(newLineIndex, firstPoint.pointIndex, secondPoint.pointIndex);
                _path.Lines.Add(newLine);
                
                _path.Points[newFirstPointIndex].lineIndices.Add(newLineIndex);
                _path.Points[newSecondPointIndex].lineIndices.Add(newLineIndex);
                
                _extraPoints.Clear();
                _extraLines.Clear();
            }
            
            if (Input.GetMouseButtonDown(0) && !_firstClickPressed && !_secondClickPressed)
            {
                _firstClickPressed = true;

                _firstPointIndex = _extraPoints.Count;
                (Vector3 pointPosition, int lineIndex) = _path.GetNearestPointOnPath(_mousePos);
                if (lineIndex != -1)
                {
                    Point point = new Point(_firstPointIndex, pointPosition);
                    _extraPoints.Add(point);
                }
                else
                {
                    Point point = new Point(_firstPointIndex, _mousePos);
                    _extraPoints.Add(point);
                }
            }
            
            if (_firstClickPressed && _secondClickPressed)
            {
                _firstClickPressed = false;
                _secondClickPressed = false;
                _secondPointCreated = false;
            }

            if (_firstClickPressed && !_secondPointCreated)
            {
                _secondPointCreated = true;
                
                _secondPointIndex = _firstPointIndex + 1;
                Point point = new Point(_secondPointIndex, _mousePos);
                _extraPoints.Add(point);

                _extraLineIndex = _extraLines.Count;
                Line line = new Line(_extraLineIndex, _firstPointIndex, _secondPointIndex);
                _extraLines.Add(line);
                
                _extraPoints[_firstPointIndex].lineIndices.Add(_extraLineIndex);
                _extraPoints[_secondPointIndex].lineIndices.Add(_extraLineIndex);
            }
            else if (_firstClickPressed && _secondPointCreated)
            {
                _extraPoints[_secondPointIndex].position = _mousePos;
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