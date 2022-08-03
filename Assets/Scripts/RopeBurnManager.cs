using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class RopeBurnManager : MonoBehaviour
{
    public Polyline line;
    public float burnSpeed = 0.5f;

    private float threshold = 0.001f;

    private bool _isBurning;

    private readonly List<Vector3> _initialPoints = new List<Vector3>();
    private readonly List<Vector3> _points = new List<Vector3>();
    private int _currentPointIndex;
    private int _nextPointIndex;
    private Vector3 _burnPoint;

    private void Start()
    {
        for (int i = 0; i < line.Count; i++)
        {
            _initialPoints.Add(line.points[i].point);
            _points.Add(line.points[i].point);
        }

        _currentPointIndex = _initialPoints.Count - 1;
        _nextPointIndex = _currentPointIndex - 1;
        _burnPoint = _initialPoints[_currentPointIndex];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || _isBurning)
        {
            _isBurning = true;
            Burn();
        }

        // if (Input.GetKey(KeyCode.Space))
        // {
        //     Burn();
        // }

        DrawLine();

        if (Input.GetKeyDown(KeyCode.R)) Reset();
    }

    private void Burn()
    {
        if (_nextPointIndex < 0 || Vector3.Distance(_burnPoint, _initialPoints[0]) == 0f)
        {
            _isBurning = false;
            return;
        }

        _burnPoint = Vector3.MoveTowards(_burnPoint, _initialPoints[_nextPointIndex], burnSpeed * Time.deltaTime);

        if (InverseLerp(_initialPoints[_nextPointIndex], _initialPoints[_currentPointIndex], _burnPoint) < threshold)
        {
            _points[_currentPointIndex] = _initialPoints[_nextPointIndex];
            _burnPoint = _initialPoints[_nextPointIndex];
            
            DrawLine();

            _points.RemoveAt(_currentPointIndex);
            
            _currentPointIndex--;
            _nextPointIndex--;
        }

        _points[_currentPointIndex] = _burnPoint;
    }

    private void DrawLine()
    {
        using (Draw.Command(Camera.main))
        {
            using (var p = new PolylinePath())
            {
                p.AddPoints(_points, Color.black);
                Draw.Polyline(p, false, 0.125f, Color.black);
            }
        }
    }

    private void Reset()
    {
        _points.Clear();
        foreach (var p in _initialPoints)
        {
            _points.Add(p);
        }

        _currentPointIndex = _initialPoints.Count - 1;
        _nextPointIndex = _currentPointIndex - 1;
        _burnPoint = _initialPoints[_currentPointIndex];
    }

    private float InverseLerp(Vector3 a, Vector3 b, Vector3 v)
    {
        float x = Vector3.Distance(v, a);
        float y = Vector3.Distance(b, a);
        return x / y;
    }
}