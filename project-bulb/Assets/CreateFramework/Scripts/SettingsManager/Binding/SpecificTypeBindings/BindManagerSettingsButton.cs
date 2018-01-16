using System;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsButton : BindableBase<ExtendedManagerSettings, object>
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
        }

        protected override object GetTargetValue()
        {
            if (Field != null && (Setting<object>)Field.GetValue(SettingsManager.Settings) != null)
            {
                return ((Setting<object>)Field.GetValue(SettingsManager.Settings)).Value;
            }
            else
            {
                throw new Exception();
            }
        }

        protected override void SetTargetValue(object value)
        {
            Setting<object> currentSetting = ((Setting<object>)Field.GetValue(SettingsManager.Settings));
            currentSetting.Value = value;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}