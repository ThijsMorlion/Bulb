using System;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsBool : BindableBase<ExtendedManagerSettings, bool>
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

        protected override bool GetTargetValue()
        {
            if (Field != null && (Setting<bool>)Field.GetValue(SettingsManager.Settings) != null)
            {
                return ((Setting<bool>)Field.GetValue(SettingsManager.Settings)).Value;
            }
            else
            {
                throw new Exception();
            }
        }

        protected override void SetTargetValue(bool value)
        {
            Setting<bool> currentSetting = ((Setting<bool>)Field.GetValue(SettingsManager.Settings));
            currentSetting.Value = value;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}