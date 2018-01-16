using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Splines.Bezier
{
    public class BezierSpline : MonoBehaviour
    {
        public event EventHandler SplineUpdated;

        public bool SuppressSplineUpdateRaise { get; set; }

        [SerializeField]
        private Vector3[] _points;  //The points array is kept private so the points at curve boundaries can be mirrored or aligned by code, so they should not be changed directly from elsewhere.
        [SerializeField]
        private BezierControlPointMode[] _modes;
        [SerializeField]
        private bool _loop;
        [SerializeField]
        private Vector3[] _rotations;

        private List<float> _lenghts = new List<float>();
        private List<Vector3> _subPoints = new List<Vector3>();

        public float TotalSplineLength { get; private set; }

        public bool Loop
        {
            get { return _loop; }
            set
            {
                _loop = value;

                if (value == true)
                {
                    _modes[_modes.Length - 1] = _modes[0];  //Match start and end point modes.
                    SetControlPoint(0, _points[0]);     //SetControlPoint will take care of position and mode constraints
                }
            }
        }

        public int ControlPointsCount { get { return _points.Length; } }
        public int CurveCount { get { return (_points.Length - 1) / 3; } }

        //Reset gets called by custom Editor classes.
        public void Reset()
        {
            _points = new Vector3[] { new Vector3(1, 0, 0), new Vector3(2, 0, 0), new Vector3(3, 0, 0), new Vector3(4, 0, 0) };
            _modes = new BezierControlPointMode[] { BezierControlPointMode.Free, BezierControlPointMode.Free };
            _rotations = new Vector3[] { Vector3.zero, Vector3.zero };

            RaiseSplineUpdated();
        }

        private void Start()
        {
            if(_subPoints.Count == 0)
            {
                CalculateSplinePoints(.1f, 10);
            }
        }

        public void RaiseSplineUpdated()
        {
            if (SuppressSplineUpdateRaise)
                return;

            if (SplineUpdated != null)
                SplineUpdated(this, EventArgs.Empty);
        }

        public Vector3 GetPoint(float t)
        {
            int curveIndex;
            if (t >= 1f)
            {
                t = 1f;
                curveIndex = _points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;  //Get the time along all curves
                curveIndex = (int)t;
                t -= curveIndex;    //Saturate t
                curveIndex *= 3;    //3 points per curve
            }

            return transform.TransformPoint(Bezier.GetPoint(_points[curveIndex], _points[curveIndex + 1], _points[curveIndex + 2], _points[curveIndex + 3], t));
        }

        public List<Vector3> CalculateSplinePoints(float distanceBetweenPoints, int steps)
        {
            _subPoints.Clear();

            for (int i = 0; i < CurveCount; i++)
            {
                // Add number of points based on the length of the curve to achieve even distribution.
                float curveLength = ApproximateCurveLength(i, steps);
                if (curveLength == 0)
                    continue;
                float step = distanceBetweenPoints / curveLength;
                if (step <= 0)
                    continue;

                float t = i == 0 ? 0 : step;
                while (t < 1)
                {
                    _subPoints.Add(GetPointOnCurve(i, t));
                    t += step;
                    if (t > 1)
                    {
                        _subPoints.Add(GetPointOnCurve(i, 1));
                        break;
                    }
                }
            }

            CalculateLenghts();
            return _subPoints;
        }

        private void CalculateLenghts()
        {
            var pointsCount = _subPoints.Count;

            _lenghts.Clear();
            var totalDistance = 0f;
            var distance = 0f;

            for (int i = 0; i < pointsCount - 1; i++)
            {
                _lenghts.Add(totalDistance);
                distance = Vector3.Distance(_subPoints[i], _subPoints[i + 1]);
                totalDistance += distance;
            }

            _lenghts.Add(totalDistance);
            TotalSplineLength = totalDistance;
        }

        public Vector3 GetSubPoint(float distance)
        {
            // Get the total distance of the spline
            var totalDistance = _lenghts[_lenghts.Count - 1];
            if (distance < 0) return _subPoints.First();
            if (distance > totalDistance) return _subPoints.Last();

            var index = 0;
            while (index < _lenghts.Count - 1 && _lenghts[index + 1] < distance)
                index++;

            var t = Mathf.InverseLerp(_lenghts[index], _lenghts[index + 1], distance);
            return Vector3.Lerp(_subPoints[index], _subPoints[index + 1], t);
        }

        public Vector3 GetPointOnCurve(int curveIndex, float t)
        {
            if (curveIndex >= CurveCount)
                return Vector3.zero;

            // 3 points per curve.
            curveIndex *= 3;

            return transform.TransformPoint(Bezier.GetPoint(_points[curveIndex], _points[curveIndex + 1], _points[curveIndex + 2], _points[curveIndex + 3], t));
        }

        public Vector3 GetRotationPoint(int index)
        {
            return _rotations[index];
        }

        public void SetRotationPoint(Vector3 rotation, int index)
        {
            _rotations[index] = rotation;
        }

        public Vector3 GetRotation(float t)
        {
            float timeOnAllCurves = Mathf.Clamp01(t) * CurveCount;  //Get the time along all curves
            int curveIndex = (int)timeOnAllCurves;
            return Vector3.Slerp(_rotations[curveIndex], _rotations[curveIndex + 1], Mathf.SmoothStep(0, 1, GetTimeOnCurve(t, curveIndex)));
        }

        public float GetClosestTimeToPoint(Vector3 point)
        {
            Dictionary<float, KeyValuePair<int, float>> closestPointAndCurveTimes = new Dictionary<float, KeyValuePair<int, float>>();

            //Loop over all bezier curves in the spline to be able to pick the curve with the point that's closest.
            for (int i = 0; i < CurveCount; i++)
            {
                float curveTime;
                float distance = Vector3.Distance(point, Bezier.ClosestPointOnBezier(_points[i * 3], _points[i * 3 + 1], _points[i * 3 + 2], _points[i * 3 + 3], point, out curveTime));
                closestPointAndCurveTimes.Add(distance, new KeyValuePair<int, float>(i, curveTime));
            }

            //Sort the values.
            closestPointAndCurveTimes.OrderBy(kv => kv.Key);

            //Return the spline time closest to the point.
            return GetTimeOnSpline(closestPointAndCurveTimes.First().Value.Value, closestPointAndCurveTimes.First().Value.Key);
        }

        public float ApproximateSplineLength(int amountOfStepsPerCurve)
        {
            float distance = 0;
            int totalStepCount = amountOfStepsPerCurve * CurveCount;
            float step = 1f / totalStepCount;
            float previousTime = 0;
            for (int i = 0; i < totalStepCount; i++)
            {
                distance += Vector3.Distance(GetPoint(previousTime + step), GetPoint(previousTime));

                previousTime += step;
            }

            return distance;
        }

        public float ApproximateCurveLength(int curveIndex, int amountOfSteps)
        {
            float distance = 0;
            float step = 1f / amountOfSteps;

            float t = 0;
            for (int i = 0; i < amountOfSteps; i++)
            {
                distance += Vector3.Distance(GetPointOnCurve(curveIndex, t + step), GetPointOnCurve(curveIndex, t));
                t += step;
            }

            return distance;
        }

        public Vector3 GetControlPoint(int index)
        {
            return _points[index];
        }

        public Vector3[] GetCurve(int index)
        {
            return new Vector3[] { _points[index * 3 + 0], _points[index * 3 + 1], _points[index * 3 + 2], _points[index * 3 + 3] };
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            //When moving a middle point, move the tangent control points along with it.
            if (index % 3 == 0)
            {
                Vector3 delta = point - _points[index];

                if (_loop)
                {
                    if (index == 0)
                    {
                        _points[1] += delta;
                        _points[_points.Length - 2] += delta;
                        _points[_points.Length - 1] = point;
                    }
                    else if (index == _points.Length - 1)
                    {
                        _points[0] = point;
                        _points[1] += delta;
                        _points[index - 1] += delta;
                    }
                    else
                    {
                        _points[index - 1] += delta;
                        _points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                    {
                        _points[index - 1] += delta;
                    }
                    if (index + 1 < _points.Length)
                    {
                        _points[index + 1] += delta;
                    }
                }

            }

            _points[index] = point;
            EnforceMode(index);

            RaiseSplineUpdated();
        }

        public BezierControlPointMode GetControlPointMode(int index)
        {
            return _modes[(index + 1) / 3];
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            int modeIndex = (index + 1) / 3;
            _modes[modeIndex] = mode;

            //Keep modes equal if the spline is set to be a loop.
            if (_loop)
            {
                if (modeIndex == 0)
                {
                    _modes[_modes.Length - 1] = mode;
                }
                else if (modeIndex == _modes.Length - 1)
                {
                    _modes[0] = mode;
                }
            }

            EnforceMode(index);
        }

        public Vector3 GetVelocity(float t)
        {
            int curveIndex;
            if (t >= 1f)
            {
                t = 1f;
                curveIndex = _points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * CurveCount;
                curveIndex = (int)t;
                t -= curveIndex;
                curveIndex *= 3;
            }
            //Velocity vector: get the derivative (amount of change), without being affected by the position.
            //The position is subtracted after TransformPoint, so a correct result is obtained regardless of scale and rotation.

            return transform.TransformPoint(Bezier.GetFirstDerivative(_points[curveIndex + 0], _points[curveIndex + 1], _points[curveIndex + 2], _points[curveIndex + 3], t)) - transform.position;
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public void AddCurve()
        {
            //Resize positions
            Vector3 point = _points[_points.Length - 1];
            Array.Resize(ref _points, _points.Length + 3);
            point.x += 1f;
            _points[_points.Length - 3] = point;
            point.x += 1f;
            _points[_points.Length - 2] = point;
            point.x += 1f;
            _points[_points.Length - 1] = point;

            Array.Resize(ref _modes, _modes.Length + 1);
            _modes[_modes.Length - 1] = _modes[_modes.Length - 2];

            EnforceMode(_points.Length - 4);

            if (_loop)
            {
                _points[_points.Length - 1] = _points[0];
                _modes[_modes.Length - 1] = _modes[0];
                EnforceMode(0);
            }

            //Resize rotations
            Array.Resize(ref _rotations, _rotations.Length + 1);

            RaiseSplineUpdated();
        }

        public void RemoveCurve()
        {
            if (_points.Length == 4)
                return;

            //Resize positions
            Array.Resize(ref _points, _points.Length - 3);
            Array.Resize(ref _modes, _modes.Length - 1);

            EnforceMode(_points.Length - 4);

            if (_loop)
            {
                _points[_points.Length - 1] = _points[0];
                _modes[_modes.Length - 1] = _modes[0];
                EnforceMode(0);
            }

            //Resize rotations
            Array.Resize(ref _rotations, _rotations.Length - 1);

            RaiseSplineUpdated();
        }

        private float GetTimeOnCurve(float t, int curveIndex)
        {
            t = Mathf.Clamp01(t) * CurveCount;  //Get the time along all curves
            t -= curveIndex;    //Saturate t

            return t;
        }

        private float GetTimeOnSpline(float t, int curveIndex)
        {
            return t / CurveCount + (1f / CurveCount) * curveIndex;
        }

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1) / 3;
            BezierControlPointMode mode = _modes[modeIndex];
            //If the mode is free, or we are at the start or end of the spline, do nothing.
            if (mode == BezierControlPointMode.Free || !_loop && (modeIndex == 0 || modeIndex == _modes.Length - 1))
                return;

            //Get the index of the point that will be adjusted. If the selected point is a point between curves, adjust one of the neighbours.
            //If the selected point is a point adjactent to a point between curves, adjust its opposing point on the other side of the middle point.
            int middleIndex = modeIndex * 3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                //Wrap around for loops
                if (fixedIndex < 0)
                    fixedIndex = _points.Length - 2;

                enforcedIndex = middleIndex + 1;
                //Wrap around for loops
                if (enforcedIndex >= _points.Length)
                    enforcedIndex = 1;
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= _points.Length)
                    fixedIndex = 1;

                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                    enforcedIndex = _points.Length - 2;
            }

            //Calculate the enforced tangent by taking the vector from the middle point to the fixed point.
            Vector3 middle = _points[middleIndex];
            Vector3 enforcedTangent = middle - _points[fixedIndex];

            //If in aligned mode, the original distance of the enforced point to the middle must be kept.
            if (mode == BezierControlPointMode.Aligned)
                enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, _points[enforcedIndex]);

            //Apply the enforced tangent - this will mirror the enforced point across the middle point.
            _points[enforcedIndex] = middle + enforcedTangent;

            RaiseSplineUpdated();
        }
    }

    public enum BezierControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }
}