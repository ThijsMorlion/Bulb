using Settings.Model;
using UnityEngine.UI;

namespace Settings.Binding.SpecificTypeBindings
{
    public class ManagerSettingsPropertyInt : ManagerSettingsProperty
    {
        public Button AddButton;
        public Button SubtractButton;

        private void OnEnable()
        {
            AddButton.onClick.AddListener(() => OnAddButtonClick());
            SubtractButton.onClick.AddListener(() => OnSubtractButtonClick());
        }

        private void OnDisable()
        {
            AddButton.onClick.RemoveAllListeners();
            SubtractButton.onClick.RemoveAllListeners();
        }

        private void OnAddButtonClick()
        {
            Setting<int> newSettings = ((Setting<int>)Field.GetValue(SettingsManager.Settings));
            newSettings.Value = newSettings.Value + 1;
            SettingsManager.Settings.GetType().GetField(Field.Name).SetValue(SettingsManager.Settings, newSettings);
        }

        private void OnSubtractButtonClick()
        {
            Setting<int> newSettings = ((Setting<int>)Field.GetValue(SettingsManager.Settings));
            newSettings.Value = newSettings.Value - 1;
            SettingsManager.Settings.GetType().GetField(Field.Name).SetValue(SettingsManager.Settings, newSettings);
        }

        public override string GetLabelTextFromField()
        {
            if (((Setting<int>)Field.GetValue(SettingsManager.Settings)).DisplayName != null) //display property available
            {
                //get the DisplayName property of the field (if there is one)
                string fieldLabelText = ((Setting<int>)Field.GetValue(SettingsManager.Settings)).DisplayName;

                if (!string.IsNullOrEmpty(fieldLabelText))
                {
                    return fieldLabelText;
                }
            }

            return CreateLabelTextFromFieldName();
        }

        public override void SetPropertiesInChildren()
        {
            foreach (var bindmanagerSetting in GetComponentsInChildren<BindManagerSettingsInt>())
            {
                bindmanagerSetting.Field = Field;
            }
        }

        public override void OnDescriptionButtonClick()
        {
            if (SettingDescription != null)
            {
                if (((Setting<int>)Field.GetValue(SettingsManager.Settings)).DisplayName != null)
                {
                    SettingDescription.TitleText = ((Setting<int>)Field.GetValue(SettingsManager.Settings)).DisplayName;
                }
                else
                {
                    SettingDescription.TitleText = Field.Name;
                }

                SettingDescription.DescriptionText = ((Setting<int>)Field.GetValue(SettingsManager.Settings)).Description;
            }
        }
    }
}