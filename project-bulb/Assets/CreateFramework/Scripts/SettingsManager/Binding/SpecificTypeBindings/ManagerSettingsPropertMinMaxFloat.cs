using Settings.Model;

namespace Settings.Binding.SpecificTypeBindings
{
    public class ManagerSettingsPropertMinMaxFloat : ManagerSettingsProperty
    {
        public override string GetLabelTextFromField()
        {
            if (((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).DisplayName != null) //display property available
            {
                //get the DisplayName property of the field (if there is one)
                string fieldLabelText = ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).DisplayName;

                if (!string.IsNullOrEmpty(fieldLabelText))
                {
                    return fieldLabelText;
                }
            }

            return CreateLabelTextFromFieldName();
        }

        public override void SetPropertiesInChildren()
        {
            foreach (var bindmanagerSetting in GetComponentsInChildren<BindManagerSettingsMinMaxFloat>())
            {
                bindmanagerSetting.Field = Field;
            }
        }

        public override void OnDescriptionButtonClick()
        {
            if (SettingDescription != null)
            {
                if (((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).DisplayName != null)
                {
                    SettingDescription.TitleText = ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).DisplayName;
                }
                else
                {
                    SettingDescription.TitleText = Field.Name;
                }

                SettingDescription.DescriptionText = ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Description;
            }
        }
    }
}