using System;
using Settings.Model;
using UnityEngine.UI;
using TMPro;

namespace Settings.Binding.SpecificTypeBindings
{
    public class ManagerSettingsPropertyButton : ManagerSettingsProperty
    {
        public Button Button;

        private void OnEnable()
        {
            Button.onClick.AddListener(() => OnButtonClick());
            PropertyChanged += ManagerSettingsPropertyButton_PropertyChanged;
        }

        private void OnDisable()
        {
            Button.onClick.RemoveAllListeners();
            PropertyChanged -= ManagerSettingsPropertyButton_PropertyChanged;
        }

        private void ManagerSettingsPropertyButton_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Field")
            {
                SetButtonContent();
            }
        }

        private void SetButtonContent()
        {
            string content = ((ButtonSetting)Field.GetValue(SettingsManager.Settings)).ButtonContent;
            if (!string.IsNullOrEmpty(content))
            {
                Button.GetComponentInChildren<TextMeshProUGUI>().text = content;
            }
        }

        private void OnButtonClick()
        {
            ((ButtonSetting)Field.GetValue(SettingsManager.Settings)).RaiseOnButtonClickEvent();
        }

        public override string GetLabelTextFromField()
        {
            if (((ButtonSetting)Field.GetValue(SettingsManager.Settings)).DisplayName != null) //display property available
            {
                //get the DisplayName property of the field (if there is one)
                string fieldLabelText = ((ButtonSetting)Field.GetValue(SettingsManager.Settings)).DisplayName;

                if (!string.IsNullOrEmpty(fieldLabelText))
                {
                    return fieldLabelText;
                }
            }

            return CreateLabelTextFromFieldName();
        }

        public override void SetPropertiesInChildren()
        {
            foreach (var bindmanagerSetting in GetComponentsInChildren<BindManagerSettingsBool>())
            {
                bindmanagerSetting.Field = Field;
            }
        }

        public override void OnDescriptionButtonClick()
        {
            if (SettingDescription != null)
            {
                if (((ButtonSetting)Field.GetValue(SettingsManager.Settings)).DisplayName != null)
                {
                    SettingDescription.TitleText = ((ButtonSetting)Field.GetValue(SettingsManager.Settings)).DisplayName;
                }
                else
                {
                    SettingDescription.TitleText = Field.Name;
                }

                SettingDescription.DescriptionText = ((ButtonSetting)Field.GetValue(SettingsManager.Settings)).Description;
            }
        }
    }
}