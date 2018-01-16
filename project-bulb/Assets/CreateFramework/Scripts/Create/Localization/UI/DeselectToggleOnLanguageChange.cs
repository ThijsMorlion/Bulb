using UnityEngine;
using UnityEngine.UI;

namespace Create.Localization.UI
{
    [RequireComponent(typeof(Toggle))]
    public class DeselectToggleOnLanguageChange : MonoBehaviour
    {
        private LanguageController _languageController;

        void OnEnable()
        {
            _languageController = GetComponentInParent<LanguageController>();
            _languageController.LanguageChanged += LanguageController_LanguageChanged;
        }

        void OnDisable()
        {
            if (_languageController != null)
                _languageController.LanguageChanged -= LanguageController_LanguageChanged;
        }

        private void LanguageController_LanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            GetComponent<Toggle>().isOn = false;
        }
    }
}