using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Settings.Binding
{
    public abstract class BindableBase<T, Y> : MonoBehaviour, IBindable
    {
        public T Target
        {
            get; set;
        }

        /// <summary>
        /// Should the UI value only be applied if the UI element is selected? This should be enabled for sliders and input fields etc, since otherwise it is not possible to bind several elements
        /// to the same target property without interference. However, elements such as toggle groups have no single element that affects the target value, so checking for selection would not yield correct
        /// results there.
        /// </summary>
        protected bool _requireUIElementSelection = true;

        protected TMP_InputField _inputField;
        protected Slider _slider;
        protected Toggle _toggle;
        protected TextMeshProUGUI _text;
        protected Y _previousTargetValue;
        protected bool _wasValueInited;

        protected abstract void SetTargetValue(Y value);
        protected abstract Y GetTargetValue();

        protected virtual void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
            _slider = GetComponent<Slider>();
            _toggle = GetComponent<Toggle>();
            _text = GetComponent<TextMeshProUGUI>();
        }

        protected virtual void LateUpdate()
        {
            if (Target == null)
                return;
            if (!_wasValueInited)
            {
                InitUIValue();
            }

            Y uiValue = GetUIValue();
            Y targetValue = GetTargetValue();

            //Only take action if bound and UI values are different.
            if (!uiValue.Equals(targetValue))
            {
                //Match the input field and the vector component.
                //If the UI value changed more recently, push the UI value to the target.
                if ((_previousTargetValue == null || _previousTargetValue.Equals(targetValue)) && CheckForUISelection())
                {
                    SetTargetValue(uiValue);
                }
                //If the target changed more recently, push its value to the UI element.
                else
                {
                    SetUIValue(targetValue);
                }
            }

            _previousTargetValue = targetValue;
        }

        private bool CheckForUISelection()
        {
            if (!_requireUIElementSelection)
            {
                return true;
            }
            else
            {
                return EventSystem.current.currentSelectedGameObject == gameObject;
            }
        }

        private void InitUIValue()
        {
            _wasValueInited = true;
            //Apply the editor time values to the UI.
            SetUIValue(GetTargetValue());
        }

        public void SetTarget(object target)
        {
            Target = (T)target;
        }

        protected virtual Y GetUIValue()
        {
            Y uiValue = default(Y);
            try
            {
                //Convert the ui element value to the target type.
                if (_inputField != null)
                {
                    if (_inputField.text == null)
                    {
                        uiValue = default(Y);
                    }
                    else
                    {
                        if (typeof(Y) == typeof(float))
                        {
                            float parcedFloat;
                            if (float.TryParse(_inputField.text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parcedFloat))
                            {
                                uiValue = (Y)Convert.ChangeType(_inputField.text, typeof(Y));
                            }
                            else
                            {
                                uiValue = default(Y);
                            }
                        }
                        if (typeof(Y) == typeof(int))
                        {
                            float parcedFloat;
                            if (float.TryParse(_inputField.text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parcedFloat))
                            {
                                uiValue = (Y)Convert.ChangeType((int)Math.Floor(parcedFloat), typeof(Y));
                            }
                            else
                            {
                                uiValue = default(Y);
                            }
                        }

                        else
                        {
                            uiValue = (Y)Convert.ChangeType(_inputField.text, typeof(Y));
                        }
                    }
                }
                else if (_slider != null)
                {
                    uiValue = (Y)Convert.ChangeType(_slider.value, typeof(Y));
                }
                else if (_toggle != null)
                {
                    uiValue = (Y)Convert.ChangeType(_toggle.isOn, typeof(Y));
                }
                else if (_text != null)
                {
                    if (_text.text == null)
                    {
                        uiValue = default(Y);
                    }
                    else
                    {
                        uiValue = (Y)Convert.ChangeType(_text.text, typeof(Y));
                    }
                }
            }
            catch
            {
                //If type conversion failed (usually only in input fields), take no action. The default value will be used.
            }

            return uiValue;
        }

        protected virtual void SetUIValue(Y targetValue)
        {
            if (_inputField != null)
            {
                if (targetValue == null)
                {
                    _inputField.text = "";
                }
                else
                {
                    _inputField.text = targetValue.ToString();
                }
            }
            else if (_slider != null)
            {
                _slider.value = (float)Convert.ChangeType(targetValue, typeof(float));
            }
            else if (_toggle != null)
            {
                _toggle.isOn = (bool)Convert.ChangeType(targetValue, typeof(bool));
            }
            else if (_text != null)
            {
                if (targetValue == null)
                {
                    _text.text = null;
                }
                else
                {
                    _text.text = targetValue.ToString();
                }
            }
        }
    }
}