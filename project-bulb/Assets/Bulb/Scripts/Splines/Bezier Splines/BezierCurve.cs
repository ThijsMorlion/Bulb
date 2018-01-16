using UnityEngine;

namespace Splines.Bezier
{
    public class BezierCurve : MonoBehaviour
    {
        public Vector3[] _points;

        //Reset gets called by custom Editor classes.
        public void Reset()
        {
            _points = new Vector3[] { new Vector3(1, 0, 0), new Vector3(2, 0, 0), new Vector3(3, 0, 0), new Vector3(4, 0, 0) };
        }

        public Vector3 GetPoint(float t)
        {
            return transform.TransformPoint(Bezier.GetPoint(_points[0], _points[1], _points[2], _points[3], t));
        }

        public Vector3 GetVelocity(float t)
        {
            //Velocity vector: get the derivative (amount of change), without being affected by the position.
            //The position is subtracted after TransformPoint, so a correct result is obtained regardless of scale and rotation.
            return transform.TransformPoint(Bezier.GetFirstDerivative(_points[0], _points[1], _points[2], _points[3], t)) - transform.position;
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }
    }
}