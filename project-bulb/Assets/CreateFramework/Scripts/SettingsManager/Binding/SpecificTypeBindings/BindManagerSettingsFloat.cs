using System;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsFloat : BindableBase<ExtendedManagerSettings, float>
    {
        private FieldInfo _field;
        public FieldInfo Field
        {
            get
            {
                return _field;
            }
            set
            {
                _field = value;
            }
        }


        private void Start()
        {
            Target = SettingsManager.Settings;
            _requireUIElementSelection = false;
        }

        protected override float GetTargetValue()
        {
            if (Field != null)
            {
                if ((Setting<float>)Field.GetValue(SettingsManager.Settings) != null)
                {
                    return ((Setting<float>)Field.GetValue(SettingsManager.Settings)).Value;
                }
                else
                {
                    throw new Exception(string.Format("Value of field on object '{0}' not found.", transform.name));
                }
            }
            else
            {
                throw new Exception(string.Format("Value of field on object '{0}' is null.", transform.name));
            }
        }

        protected override void SetTargetValue(float newValue)
        {
            Setting<float> currentSetting = (Setting<float>)Field.GetValue(SettingsManager.Settings);
            currentSetting.Value = newValue;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}