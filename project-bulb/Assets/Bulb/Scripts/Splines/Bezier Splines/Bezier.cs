using UnityEngine;

namespace Splines.Bezier
{
    public static class Bezier
    {
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            //return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t); = quadradtic bezier is the combination of 2 linear interpolations.

            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }

        /// <summary>
        /// Derivative = amount of change in a function. Provides tangents which can be interpreted as the speed with which we move along the curve.
        /// </summary>
        public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;

            return 3f * oneMinusT * oneMinusT * (p1 - p0) +
                   6f * oneMinusT * t * (p2 - p1) +
                   3f * t * t * (p3 - p2);
        }

        public static float ClosestTimeOnBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 point)
        {
            float t = ClosestTimeOnBezierInInterval(p0, p1, p2, p3, point, 0, 1, 10);
            float delta = 1.0f / 10.0f;
            for (int i = 0; i < 4; i++)
            {
                t = ClosestTimeOnBezierInInterval(p0, p1, p2, p3, point, t - delta, t + delta, 10);
                delta /= 9;//10;
            }
            return t;
        }

        public static Vector3 ClosestPointOnBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 point, out float closestTime)
        {
            closestTime = ClosestTimeOnBezier(p0, p1, p2, p3, point);
            return GetPoint(p0, p1, p2, p3, closestTime);
        }

        public static Vector3 ClosestPointOnBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 point)
        {
            float closestTime = ClosestTimeOnBezier(p0, p1, p2, p3, point);
            return GetPoint(p0, p1, p2, p3, closestTime);
        }

        /// <summary>
        /// Calculates the best fitting time in the given interval
        /// </summary>
        private static float ClosestTimeOnBezierInInterval(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 point, float aStart, float aEnd, int aSteps)
        {
            aStart = Mathf.Clamp01(aStart);
            aEnd = Mathf.Clamp01(aEnd);
            float step = (aEnd - aStart) / (float)aSteps;
            float Res = 0;
            float Ref = float.MaxValue;
            for (int i = 0; i < aSteps; i++)
            {
                float t = aStart + step * i;
                float L = (GetPoint(p0, p1, p2, p3, t) - point).sqrMagnitude;
                if (L < Ref)
                {
                    Ref = L;
                    Res = t;
                }
            }
            return Res;
        }
    }
}
