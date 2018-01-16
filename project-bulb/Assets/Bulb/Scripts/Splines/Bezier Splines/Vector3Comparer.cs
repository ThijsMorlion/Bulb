using System.Collections.Generic;
using UnityEngine;

namespace Splines.Bezier
{
    public class Vector3Comparer : IComparer<Vector3>
    {
        public int Compare(Vector3 a, Vector3 b)
        {
            if (a == b)
                return 0;

            return Vector3.Dot(b - a, Vector3.forward) > 0 ? 1 : -1;
        }
    }
}
