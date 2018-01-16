using System;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsString : BindableBase<ExtendedManagerSettings, string>
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

        protected override string GetTargetValue()
        {
            if (Field != null)
            {
                if ((Setting<string>)Field.GetValue(SettingsManager.Settings) != null)
                {
                    return ((Setting<string>)Field.GetValue(SettingsManager.Settings)).Value;
                }
                else
                {
                    throw new Exception(string.Format("Value of field on object '{0}' is null.", transform.name));
                }
            }
            else
            {
                throw new Exception(string.Format("Value of field on object '{0}' not found.", transform.name));
            }
        }

        protected override void SetTargetValue(string newValue)
        {
            Setting<string> currentSetting = (Setting<string>)Field.GetValue(SettingsManager.Settings);
            currentSetting.Value = newValue;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}