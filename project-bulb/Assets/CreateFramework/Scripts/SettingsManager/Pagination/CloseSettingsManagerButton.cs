using UnityEngine;
using UnityEngine.UI;

namespace Settings.Pagination
{
    public class CloseSettingsManagerButton : MonoBehaviour
    {
        private SettingsManagerVisibility _settingsManagerVisibility;
        private Button _button;

        private void OnEnable()
        {
            _settingsManagerVisibility = GetComponentInParent<SettingsManagerVisibility>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => OnButtonClick());
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void OnButtonClick()
        {
            _settingsManagerVisibility.SetVisibility(false);
        }
    }
}