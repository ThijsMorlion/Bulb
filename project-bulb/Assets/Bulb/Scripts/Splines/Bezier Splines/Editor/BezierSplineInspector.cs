using UnityEngine;
using UnityEditor;

namespace Splines.Bezier
{
    [CustomEditor(typeof(BezierSpline))]
    public class BezierSplineInspector : Editor
    {
        private BezierSpline _spline;
        private Transform _handlesTransform;
        private Quaternion _handlesRotation;

        private int _selectedIndex = -1;

        private const float DIRECTION_SCALE = 0.5f;
        private const float HANDLE_SIZE = 0.04f;
        private const float PICK_SIZE = 0.06f;
        private const int LINE_STEPS = 10;
        private static Color[] MODE_COLORS = { Color.white, Color.yellow, Color.cyan };

        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
            if (_spline != null)
                _spline.RaiseSplineUpdated();
        }

        void OnSceneGUI()
        {
            _spline = (BezierSpline)target;
            _handlesTransform = _spline.transform;
            _handlesRotation = Tools.pivotRotation == PivotRotation.Local ? _handlesTransform.rotation : Quaternion.identity;

            //Display the first point.
            Vector3 p0 = ShowPointWithGizmo(0);
            //Display all points in the spline, from all curves.
            for (int i = 1; i < _spline.ControlPointsCount; i += 3)
            {
                Vector3 p1 = ShowPointWithGizmo(i);
                Vector3 p2 = ShowPointWithGizmo(i + 1);
                Vector3 p3 = ShowPointWithGizmo(i + 2);

                //Draw lines between helper points
                Handles.color = Color.gray;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);

                //Draw bezier
                Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);

                p0 = p3;
            }

            //Draw direction vectors
            DrawDirections();
        }

        public override void OnInspectorGUI()
        {
            _spline = target as BezierSpline;

            //Loop
            EditorGUI.BeginChangeCheck();
            bool loop = EditorGUILayout.Toggle("Loop", _spline.Loop);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Toggle Loop");
                _spline.Loop = loop;
                EditorUtility.SetDirty(_spline);
            }

            //Point properties
            DrawSelectedPointInspector();

            //Add curve
            if (GUILayout.Button("Add Curve"))
            {
                Undo.RecordObject(_spline, "Add Curve");
                _spline.AddCurve();
                EditorUtility.SetDirty(_spline);
            }

            //Remove curve
            if (_spline.CurveCount > 1 && GUILayout.Button("Remove Curve"))
            {
                Undo.RecordObject(_spline, "Remove Curve");
                if (_selectedIndex > _spline.ControlPointsCount - 4)
                    _selectedIndex = -1;
                _spline.RemoveCurve();
                EditorUtility.SetDirty(_spline);
            }
        }

        private void DrawSelectedPointInspector()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _spline.ControlPointsCount)
                return;

            //Position
            GUILayout.Label("Selected Point");
            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Position", _spline.GetControlPoint(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Move Point");
                _spline.SetControlPoint(_selectedIndex, point);
                EditorUtility.SetDirty(_spline);
            }

            //Rotation
            if (_selectedIndex % 3 == 0)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", _spline.GetRotationPoint(_selectedIndex / 3));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_spline, "Rotate Point");
                    _spline.SetRotationPoint(rotation, _selectedIndex / 3);
                    EditorUtility.SetDirty(_spline);
                }
            }

            //Control point mode
            EditorGUI.BeginChangeCheck();
            BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", _spline.GetControlPointMode(_selectedIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_spline, "Change Point Mode");
                _spline.SetControlPointMode(_selectedIndex, mode);
                EditorUtility.SetDirty(_spline);
            }
        }

        private void DrawDirections()
        {
            Handles.color = Color.green;
            Vector3 point = _spline.GetPoint(0f);
            Handles.DrawLine(point, point + _spline.GetDirection(0f) * DIRECTION_SCALE);
            int steps = LINE_STEPS * _spline.CurveCount;
            for (int i = 1; i <= steps; i++)
            {
                point = _spline.GetPoint(i / (float)steps);
                Handles.DrawLine(point, point + _spline.GetDirection(i / (float)steps) * DIRECTION_SCALE);
            }
        }

        private Vector3 ShowPointWithGizmo(int index)
        {
            Vector3 point = _handlesTransform.TransformPoint(_spline.GetControlPoint(index));
            float size = HandleUtility.GetHandleSize(point);
            //Draw the first point bigger, so it's easier to select it in a loop (and makes it obvious where a loop begins)
            if (index == 0)
                size *= 2f;

            //Show selection gizmo
            Handles.color = MODE_COLORS[(int)_spline.GetControlPointMode(index)];
            if (Handles.Button(point, _handlesRotation, size * HANDLE_SIZE, size * PICK_SIZE, Handles.DotHandleCap))
            {
                _selectedIndex = index;
                Repaint(); //Show the newly selected point in the inspector.
            }
            //Show transform gizmo
            if (_selectedIndex == index)
            {
                Vector3 newPos = Handles.PositionHandle(point, _handlesRotation);
                if (newPos != point)
                {
                    Undo.RecordObject(_spline, "Move Point");
                    EditorUtility.SetDirty(_spline);
                    _spline.SetControlPoint(index, _handlesTransform.InverseTransformPoint(newPos));
                }
            }

            return point;
        }
    }
}