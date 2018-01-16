using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System;

namespace Splines.CatmullRom
{
    /// <summary>
    /// Creates a Catmull Rom spline between all child nodes, in order.
    /// </summary>
    [ExecuteInEditMode]
    public class CatmullRomRoot : MonoBehaviour
    {
        public int PointsPerSegment = 10;
        [Tooltip("Chordal is stiff, Uniform is floppy, Centripetal gives the best fit without creating loops.")]
        public CatmullRomType SplineType = CatmullRomType.Centripetal;
        public SplineUpdated SplineUpdated = new SplineUpdated();

        public Vector3[] PointsAlongSpline { get; private set; }

        private int _previousChildCount;

        void OnValidate()
        {
            //Clamp values within valid range.
            PointsPerSegment = Mathf.Clamp(PointsPerSegment, 3, 1000);
        }

        // Use this for initialization
        void OnEnable()
        {
            _previousChildCount = transform.childCount;
            UpdateSpline();
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.childCount != _previousChildCount)
                UpdateSpline();
        }

        public void UpdateSpline()
        {
            CatmullRomNode[] nodes = transform.GetComponentsInChildren<CatmullRomNode>();
            if (nodes != null && nodes.Length > 2)
                PointsAlongSpline = CatmullRom.Interpolate(nodes.Select(n => n.transform.position).ToList(), PointsPerSegment, SplineType).ToArray();
            else
                PointsAlongSpline = null;

            SplineUpdated.Invoke();
        }
    }

    [Serializable]
    public class SplineUpdated : UnityEvent { }
}