using Bulb.Visuals.Grid;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bulb.Core
{
    public static class ExtensionMethods
    {
        public static T GetOrAddComponent<T>(this Transform go) where T: Component
        {
            var component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.gameObject.AddComponent<T>();
                return (T)Convert.ChangeType(component, typeof(T));
            }

            return component;
        }

        public static Vector2 RotateAround(this Vector2 point, Vector2 pivot, Quaternion rotation)
        {
            return ((Vector2)(rotation * (point - pivot)) + pivot);
        }

        public static Vector2 SwitchXY(this Vector2 point)
        {
            return new Vector2(point.y, point.x);
        }

        public static Direction GetOpposite(this Direction direction)
        {
            switch(direction)
            {
                case Direction.Bottom:
                    return Direction.Top;
                case Direction.Top:
                    return Direction.Bottom;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
            }

            return direction;
        }

        public static Direction GetDirection(this Vector2 direction)
        {
            if (direction == Vector2.up)
                return Direction.Bottom;
            else if (direction == Vector2.down)
                return Direction.Top;
            else if (direction == Vector2.left)
                return Direction.Left;
            else if (direction == Vector2.right)
                return Direction.Right;

            return Direction.None;
        }

        public static bool TryFirstOrDefault<T>(this IEnumerable<T> source, out T value)
        {
            value = default(T);
            using (var iterator = source.GetEnumerator())
            {
                if (iterator.MoveNext())
                {
                    value = iterator.Current;
                    return true;
                }
                return false;
            }
        }

        public static Rect RotateRectAroundPivot(this Rect rect, Vector2 pivot, Quaternion rotation)
        {
            var rotatedRect = new Rect();

            var xyMin = new Vector2(rect.xMin, rect.yMin);
            xyMin = xyMin.RotateAround(pivot, rotation);

            var xyMax = new Vector2(rect.xMax, rect.yMax);
            xyMax = xyMax.RotateAround(pivot, rotation);

            rotatedRect.xMin = xyMin.x;
            rotatedRect.yMin = xyMin.y;
            rotatedRect.xMax = xyMax.x;
            rotatedRect.yMax = xyMax.y;

            return rotatedRect;
        }

        public static string FormatCurrent(this float value)
        {
            if (value < 1)
            {
                var mA = value * 1000;
                return string.Format("{0} mA", mA.ToString("0.00"));
            }

            return string.Format("{0} A", value.ToString("0.00"));
        }

        public static string FormatVoltage(this float value)
        {
            if (value < 1)
            {
                var mV = value * 1000;
                return string.Format("{0} mV", mV.ToString("0.00"));
            }

            return string.Format("{0} V", value.ToString("0.00"));
        }
    }
}