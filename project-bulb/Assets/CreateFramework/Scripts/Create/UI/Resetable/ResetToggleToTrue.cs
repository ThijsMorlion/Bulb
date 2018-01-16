using UnityEngine.UI;

namespace Create.UI.Resetable
{
    /// <summary>
    /// Either resets itself to true, or resets the target toggle child.
    /// </summary>
    public class ResetToggleToTrue : ResetableBase
    {
        public int TargetToggleIndex;

        public override void ResetUIElements()
        {
            if (GetComponent<Toggle>() != null)
            {
                GetComponent<Toggle>().isOn = true;
            }
            else
            {
                var toggles = GetComponentsInChildren<Toggle>();
                if (toggles != null && TargetToggleIndex < toggles.Length && TargetToggleIndex >= 0)
                {
                    toggles[TargetToggleIndex].isOn = true;
                }
            }
        }
    }
}