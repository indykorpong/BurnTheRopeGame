using System;
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
        }

        private void Update()
        {
            
        }

        private void BurnRopes()
        {
            
        }
    }
}