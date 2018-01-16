using System;
using UnityEngine;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsVector2 : BindableBase<ExtendedManagerSettings, float>
    {
        public int ThisVectorComponent;

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
            if (Field != null && (Setting<Vector2>)Field.GetValue(SettingsManager.Settings) != null)
            {
                return ((Setting<Vector2>)Field.GetValue(SettingsManager.Settings)).Value[ThisVectorComponent];
            }
            else
            {
                throw new Exception("Value of field on object '" + transform.name + "' not found or field is null.");
            }
        }

        protected override void SetTargetValue(float newValue)
        {
            Setting<Vector2> currentSetting = (Setting<Vector2>)Field.GetValue(SettingsManager.Settings);
            Vector2 vec = currentSetting.Value;
            vec[ThisVectorComponent] = newValue;
            currentSetting.Value = vec;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}