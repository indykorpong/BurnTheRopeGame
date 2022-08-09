using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

namespace BurnTheRope
{
    public class LineBurnController_old : MonoBehaviour
    {
        public Transform linesTransform;
        public float burnSpeed = 1.5f;

        private Polyline[] _polylines;
        private PolylinePath[] _polylinePaths;

        private readonly List<List<Vector3>> _initialWaypointCollection = new();
        private readonly List<List<Vector3>> _burnWaypointCollection = new();

        private List<int> _currentWaypointIndices = new();
        private List<int> _nextWaypointIndices = new();
        private List<Vector3> _burnPoints = new();
        private List<bool> _isLineBurnings = new();

        private const float Threshold = 0.001f;
        private bool _startedBurning;

        private void Start()
        {
            _polylines = linesTransform.GetComponentsInChildren<Polyline>();
            foreach (Polyline line in _polylines)
            {
                line.Color = new Color(0, 0, 0, 0);
                _initialWaypointCollection.Add(line.points.Select(x => x.point + line.transform.position).ToList());
            }

            foreach (var waypoints in _initialWaypointCollection)
            {
                _burnWaypointCollection.Add(waypoints);
                _currentWaypointIndices.Add(waypoints.Count - 1);
                _nextWaypointIndices.Add(waypoints.Count - 2);
                _burnPoints.Add(waypoints[^1]);
                _isLineBurnings.Add(false);
            }

            _polylinePaths = new PolylinePath[_polylines.Length];
            for (int i = 0; i < _polylinePaths.Length; i++)
            {
                _polylinePaths[i] = new PolylinePath();
            }

            Camera.onPreRender += DrawLines;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || _startedBurning)
            {
                for (int i = 0; i < _isLineBurnings.Count; i++)
                {
                    _isLineBurnings[i] = true;
                }

                _startedBurning = _isLineBurnings.All(x => x);

                BurnLines();
            }

            if (Input.GetKeyDown(KeyCode.R)) ResetLines();
        }

        private void BurnLines()
        {
            for (int i = 0; i < _initialWaypointCollection.Count; i++)
            {
                if (_nextWaypointIndices[i] < 0 ||
                    Vector3.Distance(_burnPoints[i], _burnWaypointCollection[i][0]) == 0f)
                {
                    // The entire line is burned. Go to burn the next line.
                    continue;
                }

                // Move the burn point towards the next waypoint.
                _burnPoints[i] = Vector3.MoveTowards(
                    _burnPoints[i],
                    _initialWaypointCollection[i][_nextWaypointIndices[i]],
                    burnSpeed * Time.deltaTime);

                /*
                 * If the current burn point reached the next waypoint,
                 * then set the position of the burn point to the position of the next waypoint,
                 * remove the burned waypoint from the list,
                 * and decrement the current and next waypoint index.
                 */
                if (InverseLerp(_burnWaypointCollection[i][_nextWaypointIndices[i]],
                        _burnWaypointCollection[i][_currentWaypointIndices[i]],
                        _burnPoints[i]) < Threshold)
                {
                    // Set the current waypoint position to the next waypoint position.
                    _burnWaypointCollection[i][_currentWaypointIndices[i]] =
                        _initialWaypointCollection[i][_nextWaypointIndices[i]];
                    _burnPoints[i] = _initialWaypointCollection[i][_nextWaypointIndices[i]];

                    _burnWaypointCollection[i].RemoveAt(_currentWaypointIndices[i]);

                    _currentWaypointIndices[i]--;
                    _nextWaypointIndices[i]--;
                }

                // Set the current waypoint to the position of the burn point.
                _burnWaypointCollection[i][_currentWaypointIndices[i]] = _burnPoints[i];
            }

            // The code below is the simplified version.
            /*
            if (_nextPointIndex < 0 || Vector3.Distance(_burnPoint, _points[0]) == 0f)
            {
                _isBurning = false;
                return;
            }
    
            _burnPoint = Vector3.MoveTowards(_burnPoint, _points[_nextPointIndex], burnSpeed * Time.deltaTime);
    
            if (InverseLerp(_points[_nextPointIndex], _points[_currentPointIndex], _burnPoint) < threshold)
            {
                _burnPoints[_currentPointIndex] = _points[_nextPointIndex];
                _burnPoint = _points[_nextPointIndex];
                
                // DrawLine();
    
                _burnPoints.RemoveAt(_currentPointIndex);
                
                _currentPointIndex--;
                _nextPointIndex--;
            }
    
            _burnPoints[_currentPointIndex] = _burnPoint;
            */
        }

        private float InverseLerp(Vector3 a, Vector3 b, Vector3 v)
        {
            float x = Vector3.Distance(v, a);
            float y = Vector3.Distance(b, a);
            return x / y;
        }

        private void DrawLines(Camera cam)
        {
            using (Draw.Command(cam))
            {
                for (int i = 0; i < _burnWaypointCollection.Count; i++)
                {
                    var points = _burnWaypointCollection[i];
                    if (points.Count == 1)
                    {
                        _isLineBurnings[i] = false;
                        continue;
                    }

                    var p = _polylinePaths[i];
                    p.ClearAllPoints();
                    p.AddPoints(points);
                    Draw.Polyline(p, false, 0.125f, Color.black);
                }
            }
        }

        private void ResetLines()
        {
            _initialWaypointCollection.Clear();
            _burnWaypointCollection.Clear();
            _currentWaypointIndices.Clear();
            _nextWaypointIndices.Clear();
            _burnPoints.Clear();

            foreach (Polyline line in _polylines)
            {
                _initialWaypointCollection.Add(line.points.Select(x => x.point + line.transform.position).ToList());
            }

            foreach (var waypoints in _initialWaypointCollection)
            {
                _burnWaypointCollection.Add(waypoints);
                _currentWaypointIndices.Add(waypoints.Count - 1);
                _nextWaypointIndices.Add(waypoints.Count - 2);
                _burnPoints.Add(waypoints[^1]);
            }

            _startedBurning = false;
        }
    }
}