using UnityEngine;

namespace Splines.CatmullRom
{
    public class AnimateAlongSpline : MonoBehaviour
    {
        public CatmullRomRoot Spline;
        public float Speed = 1, RotationSpeed = 15, DistanceAtWhichToPickNextPoint = 4;

        private int _nextPointIndex = 1;
        public bool IsAnimating { get; private set; }
        private float _sqrDistanceAtWhichToPickNextPoint;

        // Use this for initialization
        void Start()
        {
            IsAnimating = true;
            _sqrDistanceAtWhichToPickNextPoint = DistanceAtWhichToPickNextPoint * DistanceAtWhichToPickNextPoint;
            ResetPosition();
        }

        private void ResetPosition()
        {
            _nextPointIndex = 1;
            SelectNextPoint();
            transform.position = Spline.PointsAlongSpline[0];
            transform.LookAt(Spline.PointsAlongSpline[_nextPointIndex]);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ResetPosition();
                IsAnimating = true;
            }
            if (!IsAnimating || Spline.PointsAlongSpline == null)
                return;

            SelectNextPoint();
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Spline.PointsAlongSpline[_nextPointIndex] - transform.position), RotationSpeed * Time.smoothDeltaTime);
            transform.position += transform.forward * Speed * Time.smoothDeltaTime;
        }

        private void SelectNextPoint()
        {
            //If the currently selected point is close to the transform, select the next point along the spline.
            while ((transform.position - Spline.PointsAlongSpline[_nextPointIndex]).sqrMagnitude < _sqrDistanceAtWhichToPickNextPoint)
            {
                //If there is a next point available, pick it. Else, stop animating.
                if (_nextPointIndex < Spline.PointsAlongSpline.Length - 1)
                    _nextPointIndex++;
                else
                {
                    IsAnimating = false;
                    break;
                }
            }
        }
    }
}