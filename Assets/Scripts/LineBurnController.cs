using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BurnTheRope.Geometry;
using Shapes;
using UnityEngine;

namespace BurnTheRope
{
    public class LineBurnController : MonoBehaviour
    {
        public LineDrawer lineDrawer;

        private WaypointPathCollection _waypointPathCollection;

        private Camera _camera;
        private Vector3 _mousePos;

        private bool _clicked;
        private bool _finishedBurning;

        private void Start()
        {
            _waypointPathCollection = lineDrawer.WaypointPathCollection;

            _camera = FindObjectOfType<Camera>();
            _mousePos = new Vector3(0, 0, -20);

            _clicked = false;
            _finishedBurning = false;
        }

        private void Update()
        {
            _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePos.z = 0;
            
            if (Input.GetMouseButtonDown(0))
            {
                _clicked = true;
                _waypointPathCollection.SetBurnPoint(_mousePos);
            }
            
            if (_clicked && !_finishedBurning)
            {
                BurnRopes();
            }
            else if (_finishedBurning)
            {
                _clicked = false;
                _finishedBurning = false;
            }
        }

        private void BurnRopes()
        {
            _finishedBurning = _waypointPathCollection.BurnWaypointPaths();
        }
    }
}