using UnityEngine;
using UnityEngine.Events;

namespace Splines.Bezier
{
    public class SplineWalker : MonoBehaviour
    {
        public UnityEvent ArrivedAtEnd;

        public SplineWalkerMode Mode;
        public BezierSpline Spline;
        public float Speed = 3;
        [Tooltip("The speed along the bezier spline varies with length - set a reference length to average out speeds across different spline lengths.")]
        public float ReferenceSplineLength;
        public bool LookForward;

        protected float _progress, _splineLengthFactor;
        protected BezierSpline _previousSpline;
        public float CurrentDistance = 0f;

        void Start()
        {
            CalculateSplineLengthFactor();
        }

        protected virtual void Update()
        {
            if (Spline == null)
                return;

            if(_previousSpline != Spline)
            {
                _previousSpline = Spline;
                CalculateSplineLengthFactor();
                CurrentDistance = 0f;
            }

            if (CurrentDistance < Spline.TotalSplineLength)
            {
                CurrentDistance += Speed * Time.deltaTime;
            }
            else
            {
                if (Mode == SplineWalkerMode.Once)
                {
                    CurrentDistance = Spline.TotalSplineLength;

                    if (ArrivedAtEnd != null)
                        ArrivedAtEnd.Invoke();
                }
                else if (Mode == SplineWalkerMode.Loop)
                {
                    CurrentDistance = 0f;
                }
            }

            var position = Spline.GetSubPoint(CurrentDistance);
            transform.localPosition = position;

            if (LookForward)
            {
                transform.LookAt(position + Spline.GetDirection(CurrentDistance / Spline.TotalSplineLength));
            }
        }

        public void JumpToTime(float time)
        {
            _progress = Mathf.Clamp01(time);
        }

        public enum SplineWalkerMode
        {
            Once,
            Loop
        }

        protected virtual void CalculateSplineLengthFactor()
        {
            if (Spline == null || ReferenceSplineLength == 0)
            {
                _splineLengthFactor = 1;
            }
            else
            {
                var length = Spline.ApproximateSplineLength(10);
                if(length == 0)
                {
                    _splineLengthFactor = 1;
                }
                else
                {
                    _splineLengthFactor = ReferenceSplineLength / Spline.ApproximateSplineLength(10);
                }
            }
        }
    }
}