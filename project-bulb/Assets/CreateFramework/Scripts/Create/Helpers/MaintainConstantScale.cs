using UnityEngine;

namespace Create.Helpers
{
    public class MaintainConstantScale : MonoBehaviour
    {
        public Transform Target;
        public bool X = true, Y = true, Z = true;

        private Vector3 _initialScale;

        void OnEnable()
        {
            _initialScale = transform.localScale;
        }

        void Update()
        {
            if (Target == null)
                return;

            transform.localScale = new Vector3(X && Target.localScale.x != 0 ? _initialScale.x / Target.localScale.x : transform.localScale.x, 
                Y && Target.localScale.y != 0 ? _initialScale.y / Target.localScale.y : transform.localScale.y, 
                Z && Target.localScale.z != 0 ? _initialScale.z / Target.localScale.z : transform.localScale.z);
        }
    }
}