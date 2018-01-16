using UnityEngine;

namespace Create.Helpers
{
    [RequireComponent(typeof(Animator))]
    public class ControlAnimatorParameters : MonoBehaviour
    {
        public string ParamName;

        public void SetBoolParameter(bool value)
        {
            if(string.IsNullOrEmpty(ParamName))
            {
                Debug.LogWarningFormat("[{0}] No parameter name set on {1}.", GetType(), name);
                return;
            }

            GetComponent<Animator>().SetBool(ParamName, value);
        }
    }
}