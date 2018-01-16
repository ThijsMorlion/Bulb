using System.Collections.Generic;
using UnityEngine;

namespace Splines.Bezier
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(BezierSpline))]
    public class BezierSplineLineRenderer : MonoBehaviour
    {
        public float DistanceBetweenPoints = .1f;

        private const int AmountOfStepsForApproxCurveLengthCalculation = 20;

        private BezierSpline _spline;
        private List<Vector3> _points = new List<Vector3>();
        private LineRenderer _line;

        void OnEnable()
        {
            _spline = GetComponent<BezierSpline>();
            _line = GetComponent<LineRenderer>();
            if (_spline != null)
            {
                _spline.SplineUpdated += SplineUpdated;
                BuildLine();
            }
        }

        void OnDisable()
        {
            if (_spline != null)
            {
                _spline.SplineUpdated -= SplineUpdated;
            }
        }

        void OnValidate()
        {
            BuildLine();
        }

        private void SplineUpdated(object sender, System.EventArgs e)
        {
            BuildLine();
        }

        private void BuildLine()
        {
            if (_spline == null)
                return;

            _points.Clear();
            _points = _spline.CalculateSplinePoints(DistanceBetweenPoints, AmountOfStepsForApproxCurveLengthCalculation);

            if (!_line.useWorldSpace)
            {
                for(var i = 0; i < _points.Count; ++i)
                {
                    _points[i] = _line.transform.InverseTransformPoint(_points[i]);
                }
            }

            _line.positionCount = _points.Count;
            _line.SetPositions(_points.ToArray());
        }
    }
}