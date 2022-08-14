using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

namespace BurnTheRope.Geometry
{
    public class Path
    {
        private List<Line> _lines;
        private List<Point> _points;

        private void InitPath(Transform polylinesTransform)
        {
            Polyline[] polylines = polylinesTransform.GetComponentsInChildren<Polyline>();

            int pointIndex = 0;
            int lineIndex = 0;
            foreach (Polyline polyline in polylines)
            {
                var positions = polyline.points.Select(x => x.point + polyline.transform.position).ToList();
                polyline.gameObject.SetActive(false);

                if (positions.Count > 1)
                {
                    foreach(Vector3 position in positions)
                    {
                        Point point = new Point(pointIndex++, position);
                        _points.Add(point);
                    }

                    for (int i = 0; i < _points.Count - 1; i++)
                    {
                        Line line = new Line(lineIndex, _points[i].pointIndex, _points[i + 1].pointIndex);
                        _lines.Add(line);
                        _points[i].AddLineIndex(lineIndex);
                        _points[i + 1].AddLineIndex(lineIndex);

                        lineIndex++;
                    }
                }
            }
        }
    }
}