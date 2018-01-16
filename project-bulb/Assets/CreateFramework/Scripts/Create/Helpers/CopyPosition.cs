using UnityEngine;

namespace Create.Helpers
{
    public class CopyPosition : MonoBehaviour
    {
        public Transform Target;
        public PositionCopyTypes CopyType;

        void Start()
        {
            if (Target == null)
            {
                Debug.LogWarningFormat("No target assigned in {0} {1}.", GetType(), name);
            }
        }

        void Update()
        {
            if (Target == null)
                return;

            if (CopyType == PositionCopyTypes.World)
            {
                transform.position = Target.position;
            }
        }
    }

    public enum PositionCopyTypes
    {
        World
    }
}