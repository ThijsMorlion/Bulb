using UnityEngine;
using System.Reflection;
using System.ComponentModel;
using Settings.PasscodePanel;

namespace Settings.Pagination
{
    public class SettingsManagerPagesController : MonoBehaviour
    {
        private SettingsManagerVisibility _settingsManagerVisibility;
        private SettingsManagerPasswordPanel _settingsManagerPasswordPanel;

        private SettingsManagerPages _selectedPage;
        public SettingsManagerPages SelectedPage
        {
            get
            {
                return _selectedPage;
            }
            set
            {
                _selectedPage = value;
                OnPropertyChanged(MethodBase.GetCurrentMethod().Name.Substring(4));
            }
        }

        public enum SettingsManagerPages
        {
            PasswordPanel,
            SettingsList,
            LogMessages,
        }

        private void OnEnable()
        {
            _settingsManagerVisibility = GetComponent<SettingsManagerVisibility>();
            _settingsManagerPasswordPanel = GetComponentInChildren<SettingsManagerPasswordPanel>();
            _settingsManagerVisibility.PropertyChanged += SettingsManagerVisibility_PropertyChanged;
        }

        private void OnDisable()
        {
            _settingsManagerVisibility.PropertyChanged -= SettingsManagerVisibility_PropertyChanged;
        }

        private void SettingsManagerVisibility_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSettingsManagerVisible")
            {
                if (_settingsManagerVisibility.IsSettingsManagerVisible)
                {
                    SetStartSettingsManagerPage();
                }
            }
        }

        private void SetStartSettingsManagerPage()
        {
            if (!_settingsManagerPasswordPanel.IsPasswordSet() && SettingsManager.Settings.SettingsPasscode.Value.Length > 0)
            {
                Debug.LogWarning("Passcode is entered in settings but all characters are non-numeric, because of this the passcode panel will not be shown. Check the description of the Passcode settings for more information.");
            }

            if (_settingsManagerPasswordPanel.IsPasswordSet())
            {
                SelectedPage = SettingsManagerPages.PasswordPanel;
            }
            else
            {
                SelectedPage = SettingsManagerPages.SettingsList;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
