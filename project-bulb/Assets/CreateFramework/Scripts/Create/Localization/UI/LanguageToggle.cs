using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Create.UI.Fonts;
using Create.Localization.Models;

namespace Create.Localization.UI
{
    [RequireComponent(typeof(Toggle))]
    public class LanguageToggle : MonoBehaviour
    {
        private Language _language;
        public Language Language
        {
            get { return _language; }
            set
            {
                _language = value;
                UpdateLabel();
            }
        }

        private LanguageController _languageController;

        void OnEnable()
        {
            GetComponent<Toggle>().onValueChanged.AddListener(ToggleChanged);
            _languageController = GetComponentInParent<LanguageController>();
            _languageController.LanguageChanged += LanguageController_LanguageChanged;

            UpdateToggleToSelectedLanguage();
        }

        void OnDisable()
        {
            GetComponent<Toggle>().onValueChanged.RemoveListener(ToggleChanged);
            if (_languageController != null)
                _languageController.LanguageChanged -= LanguageController_LanguageChanged;
        }

        private void LanguageController_LanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            // Update toggle on language change.
            UpdateToggleToSelectedLanguage();
        }

        private void UpdateToggleToSelectedLanguage()
        {
            if (_languageController == null || _languageController.SelectedLanguage == null)
                return;

            GetComponent<Toggle>().onValueChanged.RemoveListener(ToggleChanged);
            GetComponent<Toggle>().isOn = Language == _languageController.SelectedLanguage;
            GetComponent<Toggle>().onValueChanged.AddListener(ToggleChanged);
        }

        private void ToggleChanged(bool value)
        {
            if (_languageController == null)
                return;

            if (value)
            {
                _languageController.SelectedLanguage = Language;
            }
        }

        private void UpdateLabel()
        {
            var text = GetComponentInChildren<TextMeshProUGUI>();
            if (text == null)
                return;
            if(Language == null)
            {
                text.text = "null";
                return;
            }

            text.text = Language.Title;

            // Apply font settings.
            var fontController = FindObjectOfType<FontController>();
            if(fontController != null)
            {
                var fontInfo = fontController.GetByCulture(Language.Culture);
                text.font = fontInfo.Font;
                text.isRightToLeftText = fontInfo.IsRightToLeft;
            }
        }
    }
}