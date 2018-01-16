using Settings.Model;

namespace Settings.Binding.SpecificTypeBindings
{
    public class ManagerSettingsPropertyFloat : ManagerSettingsProperty
    {
        public override string GetLabelTextFromField()
        {
            if (((Setting<float>)Field.GetValue(SettingsManager.Settings)).DisplayName != null) //display property available
            {
                //get the DisplayName property of the field (if there is one)
                string fieldLabelText = ((Setting<float>)Field.GetValue(SettingsManager.Settings)).DisplayName;

                if (!string.IsNullOrEmpty(fieldLabelText))
                {
                    return fieldLabelText;
                }
            }
            return CreateLabelTextFromFieldName();
        }

        public override void SetPropertiesInChildren()
        {
            foreach (var bindmanagerSetting in GetComponentsInChildren<BindManagerSettingsFloat>())
            {
                bindmanagerSetting.Field = Field;
            }
        }

        public override void OnDescriptionButtonClick()
        {
            if (SettingDescription != null)
            {
                if (((Setting<float>)Field.GetValue(SettingsManager.Settings)).DisplayName != null)
                {
                    SettingDescription.TitleText = ((Setting<float>)Field.GetValue(SettingsManager.Settings)).DisplayName;
                }
                else
                {
                    SettingDescription.TitleText = Field.Name;
                }

                SettingDescription.DescriptionText = ((Setting<float>)Field.GetValue(SettingsManager.Settings)).Description;
            }
        }
    }
}