using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Settings.PasscodePanel
{
    public class SettingsManagerPasswordNumber : MonoBehaviour
    {
        public string Number;

        private SettingsManagerPasswordPanel _settingsManagerPasswordPanel;
        private Button _button;
        private TextMeshProUGUI _text;

        private void OnEnable()
        {
            _settingsManagerPasswordPanel = GetComponentInParent<SettingsManagerPasswordPanel>();

            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnButtonClick());

            _text = GetComponent<TextMeshProUGUI>();
            _text.text = Number;
        }

        private void OnButtonClick()
        {
            _settingsManagerPasswordPanel.OnNumberEntered(Number);
        }
    }
}
