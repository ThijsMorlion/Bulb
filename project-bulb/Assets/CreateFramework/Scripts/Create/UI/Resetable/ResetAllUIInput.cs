using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Create.UI.Resetable
{
    public class ResetAllUIInput : MonoBehaviour
    {
        public bool OnlyResetCustomResetables;

        public void ResetAll()
        {
            if (!OnlyResetCustomResetables)
            {
                // Regular input fields.
                InputField[] inputFields = FindObjectsOfType<InputField>();
                foreach (var inputField in inputFields)
                {
                    if (inputField.GetComponent<DontReset>() != null)
                        return;
                    inputField.text = "";
                }

                // Dropdowns.
                Dropdown[] dropDowns = FindObjectsOfType<Dropdown>();
                foreach (var dropDown in dropDowns)
                {
                    if (dropDown.GetComponent<DontReset>() != null)
                        return;
                    dropDown.value = 0;
                }

                // Toggles and toggle groups.
                ResetToggles();

                // Scroll rects.
                var scrollRects = FindObjectsOfType<ScrollRect>();
                foreach (var scrollRect in scrollRects)
                {
                    scrollRect.normalizedPosition = new Vector2(0, 1);
                }
            }

            // Custom resetables.
            foreach (var resetable in FindObjectsOfType(typeof(ResetableBase)))
            {
                ((ResetableBase)resetable).ResetUIElements();
            }
        }

        private static void ResetToggles()
        {
            // Allow complete switch off in toggle groups.
            ToggleGroup[] toggleGroups = FindObjectsOfType<ToggleGroup>().Where(t => t.GetComponent<DontReset>() == null).ToArray();
            bool[] previousSwitchOffState = new bool[toggleGroups.Length];
            for (int i = 0; i < toggleGroups.Length; i++)
            {
                previousSwitchOffState[i] = toggleGroups[i].allowSwitchOff;
                toggleGroups[i].allowSwitchOff = true;
            }

            // Deactivate all toggles (both with and without toggle groups)
            Toggle[] toggles = FindObjectsOfType<Toggle>();
            foreach (Toggle toggle in toggles)
            {
                if (toggle.GetComponent<DontReset>() != null)
                    continue;
                if (toggle.group != null && toggle.group.GetComponent<DontReset>() != null)
                    continue;
                toggle.isOn = false;
            }

            // Reset the switch off state of the toggles.
            for (int i = 0; i < toggleGroups.Length; i++)
            {
                toggleGroups[i].allowSwitchOff = previousSwitchOffState[i];
            }
        }
    }
}