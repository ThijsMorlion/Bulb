using TMPro;
using UnityEngine;
using System.Linq;

namespace Create.Localization.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class DisplayPluralInterfaceText : DisplayInterfaceText
    {
        public TextMeshProUGUI Target;
        public string SingularKey, PluralKey;

        private int _value;

        void Update()
        {
            // Update the text when the target value changes.
            if (Target == null || string.IsNullOrEmpty(Target.text))
                return;

            int newValue;
            int.TryParse(Target.text, out newValue);
            if (newValue != _value)
            {
                UpdateText();
                _value = newValue;
            }
        }

        protected override string GetLocalizedText()
        {
            int value;
            if (!int.TryParse(Target.text, out value))
            {
                return "Target text is NaN.";
            }
            else
            {
                if (value == 1)
                {
                    return GetInterfaceText(SingularKey);
                }
                else
                {
                    return GetInterfaceText(PluralKey);
                }
            }
        }

        private string GetInterfaceText(string key)
        {
            var intText = LanguageController.InterfaceTexts.Where(i => i.Key == key).FirstOrDefault();
            if (intText == null)
            {
                return null;
            }
            else
            {
                return intText[_languageController.SelectedLanguage.Culture];
            }
        }
    }
}