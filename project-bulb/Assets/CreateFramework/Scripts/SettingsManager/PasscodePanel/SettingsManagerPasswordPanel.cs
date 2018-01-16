using System;
using UnityEngine;
using Settings.Model;
using Settings.Pagination;
using Create.UI.Tweenables;

namespace Settings.PasscodePanel
{
    public class SettingsManagerPasswordPanel : MonoBehaviour
    {
        public GameObject InputIndicatorPrefab;
        public Transform IndicatorsTransform;

        private SettingsManagerPagesController _settingsManagerPagesController;
        private string _currentEnteringPassword;
        private string _masterPasscode = "1987";
        private string _lastFourEnteredNumbers = "";

        private void OnEnable()
        {
            _settingsManagerPagesController = GetComponentInParent<SettingsManagerPagesController>();
            _settingsManagerPagesController.PropertyChanged += SettingsManagerPagesController_PropertyChanged;
            SettingsManager.Settings.SettingsPasscode.PropertyChanged += SettingsPassword_PropertyChanged;
            CreateInputIndicators();
        }

        private void OnDisable()
        {
            SettingsManager.Settings.SettingsPasscode.PropertyChanged -= SettingsPassword_PropertyChanged;
        }

        private void SettingsManagerPagesController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedPage")
            {
                if (_settingsManagerPagesController.SelectedPage == SettingsManagerPagesController.SettingsManagerPages.PasswordPanel)
                {
                    ResetCurrentlyInputtedPassword();
                }
            }
        }

        private void SettingsPassword_PropertyChanged(object sender, SettingChangedEventArgs<string> e)
        {
            CreateInputIndicators();
        }

        private void CreateInputIndicators()
        {
            DestroyExistingIndicators(IndicatorsTransform);

            foreach (char c in GetNumericPasscode())
            {
                Instantiate(InputIndicatorPrefab, IndicatorsTransform, false);
            }
        }

        private void DestroyExistingIndicators(Transform trans)
        {
            foreach (Transform child in trans)
            {
                Destroy(child.gameObject);
            }
        }

        public void OnNumberEntered(string number)
        {
            _currentEnteringPassword += number;
            AddToLastFourNumbersEntered(number);
            CheckIfMasterPassword();

            FillInputIndicators();
            CheckIfCorrectPassword();
        }

        private void AddToLastFourNumbersEntered(string number)
        {
            if (_lastFourEnteredNumbers.Length >= 4)
            {
                _lastFourEnteredNumbers = _lastFourEnteredNumbers.Remove(0, 1);
            }
            _lastFourEnteredNumbers += number;
        }

        private void CheckIfMasterPassword()
        {
            if (_lastFourEnteredNumbers == _masterPasscode)
            {
                OnCorrectPasscodeEntered();
            }
        }

        private void FillInputIndicators()
        {
            for (int i = 0; i < IndicatorsTransform.childCount; i++)
            {
                if (i < _currentEnteringPassword.Length)
                {
                    IndicatorsTransform.GetChild(i).GetComponent<TweenableGroup>().TweenToState(true);
                }
                else
                {
                    IndicatorsTransform.GetChild(i).GetComponent<TweenableGroup>().TweenToState(false);
                }
            }
        }

        private void CheckIfCorrectPassword()
        {
            if (_currentEnteringPassword == GetNumericPasscode())
            {
                OnCorrectPasscodeEntered();
            }
            else if (_currentEnteringPassword.Length == GetNumericPasscode().Length)
            {
                //wrong password, restart
                ResetCurrentlyInputtedPassword();
            }
        }

        private void OnCorrectPasscodeEntered()
        {
            _settingsManagerPagesController.SelectedPage = SettingsManagerPagesController.SettingsManagerPages.SettingsList;
            _currentEnteringPassword = "";
            _lastFourEnteredNumbers = "";
        }

        private void ResetCurrentlyInputtedPassword()
        {
            _currentEnteringPassword = "";
            FillInputIndicators();
        }

        public bool IsPasswordSet()
        {
            return !string.IsNullOrEmpty(GetNumericPasscode());
        }

        private string GetNumericPasscode()
        {
            string numericPassword = "";

            foreach (char c in SettingsManager.Settings.SettingsPasscode.Value)
            {
                if (Char.IsDigit(c))
                {
                    numericPassword += c;
                }
            }

            return numericPassword;
        }
    }
}