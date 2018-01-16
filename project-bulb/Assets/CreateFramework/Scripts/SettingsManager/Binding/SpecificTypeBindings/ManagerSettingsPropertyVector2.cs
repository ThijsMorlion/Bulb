using UnityEngine;
using Settings.Model;

namespace Settings.Binding.SpecificTypeBindings
{
    public class ManagerSettingsPropertyVector2 : ManagerSettingsProperty
    {
        public override string GetLabelTextFromField()
        {
            if (((Setting<Vector2>)Field.GetValue(SettingsManager.Settings)).DisplayName != null) //display property available
            {
                //get the DisplayName property of the field (if there is one)
                string fieldLabelText = ((Setting<Vector2>)Field.GetValue(SettingsManager.Settings)).DisplayName;

                if (!string.IsNullOrEmpty(fieldLabelText))
                {
                    return fieldLabelText;
                }
            }

            return CreateLabelTextFromFieldName();
        }

        public override void SetPropertiesInChildren()
        {
            foreach (var bindmanagerSetting in GetComponentsInChildren<BindManagerSettingsVector2>())
            {
                bindmanagerSetting.Field = Field;
            }
        }

        public override void OnDescriptionButtonClick()
        {
            if (SettingDescription != null)
            {
                if (((Setting<Vector2>)Field.GetValue(SettingsManager.Settings)).DisplayName != null)
                {
                    SettingDescription.TitleText = ((Setting<Vector2>)Field.GetValue(SettingsManager.Settings)).DisplayName;
                }
                else
                {
                    SettingDescription.TitleText = Field.Name;
                }

                SettingDescription.DescriptionText = ((Setting<Vector2>)Field.GetValue(SettingsManager.Settings)).Description;
            }
        }

        public override void SetVectorDimensionsInChildren()
        {
            BindManagerSettingsVector2[] bindManagerComponents = GetComponentsInChildren<BindManagerSettingsVector2>();
            for (int i = 0; i < bindManagerComponents.Length; i++)
            {
                bindManagerComponents[i].ThisVectorComponent = i; //assigns the dimension (x,y,z,...) to the component
            }
        }
    }
}