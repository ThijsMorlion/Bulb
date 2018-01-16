using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Settings.Model;
using System.Reflection;

namespace Settings.Binding.SpecificTypeBindings
{
    public class BindManagerSettingsMinMaxFloat : BindableBase<ExtendedManagerSettings, float>
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
                SetElementProperties();
            }
        }

        private void Start()
        {
            Target = SettingsManager.Settings;
            _requireUIElementSelection = true;
        }

        private void SetElementProperties()
        {
            Slider slider = GetComponent<Slider>();
            TMP_InputField inputfield = GetComponent<TMP_InputField>();

            if (slider != null)
            {
                SetSliderRange(slider);
            }

            if (inputfield != null)
            {
                SetStartInputText(inputfield);
            }
        }

        private void SetSliderRange(Slider slider)
        {
            slider.minValue = ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Min;
            slider.maxValue = ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Max;
        }

        private void SetStartInputText(TMP_InputField inputfield)
        {
            inputfield.text = ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Value.ToString();
        }

        protected override float GetTargetValue()
        {
            if (Field != null && (MinMaxSetting)Field.GetValue(SettingsManager.Settings) != null)
            {
                return ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Value;
            }
            else
            {
                throw new Exception("Value of field on object '" + transform.name + "' not found or field not set.");
            }
        }

        protected override void SetTargetValue(float newValue)
        {
            //keep the value between the set bounds
            newValue = Mathf.Clamp(newValue, ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Min, ((MinMaxSetting)Field.GetValue(SettingsManager.Settings)).Max);

            //set value
            MinMaxSetting currentSetting = (MinMaxSetting)Field.GetValue(SettingsManager.Settings);
            currentSetting.Value = newValue;
            Field.SetValue(SettingsManager.Settings, currentSetting);
        }
    }
}