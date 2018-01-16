using UnityEngine;
using UnityEngine.UI;

namespace Settings.Pagination
{
    public class GoToSettingsManagerPageOnButtonClick : MonoBehaviour
    {
        public SettingsManagerPagesController.SettingsManagerPages PageToGoTo;

        private SettingsManagerPagesController _settingsManagerPagesController;
        private Button _button;

        private void OnEnable()
        {
            _settingsManagerPagesController = GetComponentInParent<SettingsManagerPagesController>();
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => GoToPage());
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void GoToPage()
        {
            _settingsManagerPagesController.SelectedPage = PageToGoTo;
        }
    }
}