using System;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsInt : BindableBase<ExtendedManagerSettings, int>
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

        protected override int GetTargetValue()
        {
            if (Field != null && (Setting<int>)Field.GetValue(SettingsManager.Settings) != null)
            {
                return ((Setting<int>)Field.GetValue(SettingsManager.Settings)).Value;
            }
            else
            {
                throw new Exception(string.Format("Value of field on object '{0}' not found or field is null.", transform.name));
            }
        }

        protected override void SetTargetValue(int newValue)
        {
            Setting<int> currentSetting = (Setting<int>)Field.GetValue(SettingsManager.Settings);
            currentSetting.Value = newValue;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}