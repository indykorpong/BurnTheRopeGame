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
        public Transform polylineTransforms;

        private WaypointPath _waypointPath;
        private WaypointPathCollection _waypointPathCollection;
        
        private Camera _camera;
        private Vector3 _mousePos;
        
        private bool _clicked;
        private bool _finishedBurning;

        public const float CLICK_POINT_RADIUS = 0.2f;
        
        private void Start()
        {
            var polylines = polylineTransforms.GetComponentsInChildren<Polyline>();
            _waypointPathCollection = new WaypointPathCollection();
            
            foreach (Polyline polyline in polylines)
            {
                var pointList = polyline.points.Select(x => x.point + polyline.transform.position).ToList();
                polyline.gameObject.SetActive(false);

                _waypointPath = new WaypointPath(pointList);
                _waypointPathCollection.AddWaypointPath(_waypointPath);
            }
            
            Camera.onPreRender += _waypointPathCollection.DrawWaypointPaths;
            Camera.onPreRender += DrawClickPoint;
            
            _camera = FindObjectOfType<Camera>();
            _mousePos = new Vector3(0, 0, -20);

            _clicked = false;
            _finishedBurning = false;
        }

        private void Update()
        {
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

        private void DrawClickPoint(Camera cam)
        {
            if (Input.GetMouseButton(0))
            {
                _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                _mousePos.z = 0;
                _clicked = true;
                
                using (Draw.Command(cam))
                {
                    Draw.Disc(_mousePos, Quaternion.identity, 0.2f, Color.red);
                }
            }
        }

        private void BurnRopes()
        {
            _finishedBurning = _waypointPathCollection.BurnWaypointPaths(_mousePos);
        }
    }
}