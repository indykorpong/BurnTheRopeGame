using System.Collections.Generic;
using System.Linq;
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

        public void SetBurnPoint(Vector3 mousePos)
        {
            foreach (WaypointPath waypointPath in _waypointPaths)
            {
                waypointPath.SetBurnPoint(mousePos);
            }
        }

        public bool BurnWaypointPaths()
        {
            bool[] finishedBurnings = new bool[_waypointPaths.Count];
            for (int i = 0; i < finishedBurnings.Length; i++)
            {
                finishedBurnings[i] = false;
            }
            
            for (int i = 0; i < _waypointPaths.Count; i++)
            {
                if (finishedBurnings[i]) continue;
                finishedBurnings[i] = _waypointPaths[i].BurnLines();
            }

            return finishedBurnings.All(x => x);
        }
    }
}