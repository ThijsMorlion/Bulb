using UnityEngine;
using TMPro;

namespace Create.Localization.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DisplaySelectedLanguageName : MonoBehaviour
    {
        private LanguageController _languageController;

        void OnEnable()
        {
            _languageController = GetComponentInParent<LanguageController>();

            if(_languageController == null)
            {
                Debug.LogWarningFormat("{0} {1} must be parented to a {2}.", GetType(), gameObject.name, typeof(LanguageController));
                return;
            }

            _languageController.LanguageChanged += LanguageController_LanguageChanged;
            DisplayLanguage();
        }

        void OnDisable()
        {
            if (_languageController != null)
                _languageController.LanguageChanged -= LanguageController_LanguageChanged;
        }

        private void LanguageController_LanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            DisplayLanguage();
        }

        private void DisplayLanguage()
        {
            if(_languageController.SelectedLanguage == null)
            {
                GetComponent<TextMeshProUGUI>().text = null;
            }
            else
            {
                GetComponent<TextMeshProUGUI>().text = _languageController.SelectedLanguage.Title;
            }
        }
    }
}