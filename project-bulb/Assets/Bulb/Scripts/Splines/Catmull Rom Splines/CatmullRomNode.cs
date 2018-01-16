using UnityEngine;

namespace Splines.CatmullRom
{
    [ExecuteInEditMode]
    public class CatmullRomNode : MonoBehaviour
    {
        private Vector3 _previousPosition;
        private CatmullRomRoot _splineRoot;

        void OnEnable()
        {
            _previousPosition = transform.position;

            CatmullRomRoot root = transform.GetComponentInParent<CatmullRomRoot>();
            if (root != null)
                _splineRoot = root;
            else
                Debug.LogError("Spline nodes must be parented to a root.");
        }

        void Update()
        {
            if (_splineRoot == null)
                return;

            if (transform.position != _previousPosition)
                _splineRoot.UpdateSpline();
        }
    }
}