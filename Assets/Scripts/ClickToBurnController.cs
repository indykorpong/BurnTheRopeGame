using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class ClickToBurnController : MonoBehaviour
{
    private Camera _camera;
    private Vector3 _mousePos;
    private void Start()
    {
        Camera.onPreRender += SpawnPoint;
        _camera = FindObjectOfType<Camera>();

        _mousePos = new Vector3(0, 0, -20);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        
        _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0;
    }

    private void SpawnPoint(Camera cam)
    {
        using (Draw.Command(cam))
        {
            Draw.Disc(_mousePos, Quaternion.identity, 0.2f, Color.red);
        }
    }
}