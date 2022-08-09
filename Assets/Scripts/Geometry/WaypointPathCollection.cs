using System.Collections.Generic;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    public class WaypointPathCollection
    {
        private List<WaypointPath> _waypointPaths = new List<WaypointPath>();

        public WaypointPathCollection()
        {
            
        }

        public void AddWaypointPath(WaypointPath waypointPath)
        {
            _waypointPaths.Add(waypointPath);
        }

        public void DrawWaypointPaths(Camera cam)
        {
            foreach (WaypointPath waypointPath in _waypointPaths)
            {
                waypointPath.DrawLines(cam);
            }
        }
    }
}