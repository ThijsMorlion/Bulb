using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Splines.Bezier
{
    [CustomEditor(typeof(BezierCurve))]
    public class BezierCurveInspector : Editor
    {
        private BezierCurve _curve;
        private Transform _handlesTransform;
        private Quaternion _handlesRotation;

        private const float DirectionScale = 0.5f;

        private const int LineSteps = 10;

        void OnSceneGUI()
        {
            _curve = (BezierCurve)target;
            _handlesTransform = _curve.transform;
            _handlesRotation = Tools.pivotRotation == PivotRotation.Local ? _handlesTransform.rotation : Quaternion.identity;

            Vector3 p0 = ShowPointWithGizmo(0);
            Vector3 p1 = ShowPointWithGizmo(1);
            Vector3 p2 = ShowPointWithGizmo(2);
            Vector3 p3 = ShowPointWithGizmo(3);

            //Draw lines between helper points
            Handles.color = Color.gray;
            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p3);

            //Draw bezier
            Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);

            //Draw direction vectors
            DrawDirections();
        }

        private void DrawDirections()
        {
            Handles.color = Color.green;
            Vector3 point = _curve.GetPoint(0f);
            Handles.DrawLine(point, point + _curve.GetDirection(0f) * DirectionScale);
            for (int i = 1; i <= LineSteps; i++)
            {
                point = _curve.GetPoint(i / (float)LineSteps);
                Handles.DrawLine(point, point + _curve.GetDirection(i / (float)LineSteps) * DirectionScale);
            }

        }

        private Vector3 ShowPointWithGizmo(int index)
        {
            Vector3 point = _handlesTransform.TransformPoint(_curve._points[index]);
            Vector3 newPos = Handles.PositionHandle(point, _handlesRotation);
            if (newPos != point)
            {
                Undo.RecordObject(_curve, "Move Point");
                EditorUtility.SetDirty(_curve);
                _curve._points[index] = newPos;
            }

            return point;
        }
    }
}