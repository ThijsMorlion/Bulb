using System;
using UnityEngine;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsVector3 : BindableBase<ExtendedManagerSettings, float>
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
            if (Field != null && (Setting<Vector3>)Field.GetValue(SettingsManager.Settings) != null)
            {
                return ((Setting<Vector3>)Field.GetValue(SettingsManager.Settings)).Value[ThisVectorComponent];
            }
            else
            {
                throw new Exception("Value of field on object '" + transform.name + "' not found or field is null.");
            }
        }

        protected override void SetTargetValue(float newValue)
        {
            Setting<Vector3> currentSetting = (Setting<Vector3>)Field.GetValue(SettingsManager.Settings);
            Vector3 vec = currentSetting.Value;
            vec[ThisVectorComponent] = newValue;
            currentSetting.Value = vec;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}