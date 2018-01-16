using UnityEngine;
using UnityEditor;

namespace Splines.CatmullRom
{
    public static class DrawSplines
    {
        [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
        static void RenderCustomGizmo(CatmullRomRoot splineRoot, GizmoType gizmoType)
        {
            DrawSpline(splineRoot.PointsAlongSpline);
            //DrawNodes(splineRoot.GetComponentsInChildren<CatmullRomNode>());
        }

        private static void DrawSpline(Vector3[] pointsAlongSpline)
        {
            if (pointsAlongSpline == null)
                return;
            Handles.DrawAAPolyLine(pointsAlongSpline);
        }

        private static void DrawNodes(CatmullRomNode[] catmullRomNodes)
        {
            for (int i = 0; i < catmullRomNodes.Length; i++)
            {
                if (HandleUtility.WorldToGUIPoint(catmullRomNodes[i].transform.position - Input.mousePosition).sqrMagnitude < 1f)
                    Handles.color = Color.cyan;
                else
                    Handles.color = Color.white;
                Handles.SphereHandleCap(i, catmullRomNodes[i].transform.position, Quaternion.identity, 0.1f, EventType.Repaint);
            }
        }
    }
}