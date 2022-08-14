using System;
using System.Linq;
using BurnTheRope.Geometry;
using Shapes;
using UnityEngine;

namespace BurnTheRope
{
    public class LineDrawer : MonoBehaviour
    {
        public Transform polylinesTransform;
        
        private WaypointPathCollection _waypointPathCollection;
        public WaypointPathCollection WaypointPathCollection
        {
            get => _waypointPathCollection;
            set => _waypointPathCollection = value;
        }

        private WaypointPath _waypointPath;

        private Camera _camera;
        private Vector3 _mousePos;
        public const float CLICK_POINT_RADIUS = 0.2f;
        
        private void Awake()
        {
            _waypointPathCollection = new WaypointPathCollection();
            
            var polylines = polylinesTransform.GetComponentsInChildren<Polyline>();

            foreach (Polyline polyline in polylines)
            {
                var pointList = polyline.points.Select(x => x.point + polyline.transform.position).ToList();
                polyline.gameObject.SetActive(false);

                _waypointPath = new WaypointPath(pointList);
                _waypointPathCollection.AddWaypointPath(_waypointPath);
            }
        }

        private void Start()
        {
            Camera.onPreRender += _waypointPathCollection.DrawWaypointPaths;
            Camera.onPreRender += DrawClickPoint;
        }

        private void Update()
        {
            _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _mousePos.z = 0;
        }
        
        private void DrawClickPoint(Camera cam)
        {
            using (Draw.Command(cam))
            {
                Draw.Disc(_mousePos, Quaternion.identity, CLICK_POINT_RADIUS, Color.red);
            }
        }
        
    }
}