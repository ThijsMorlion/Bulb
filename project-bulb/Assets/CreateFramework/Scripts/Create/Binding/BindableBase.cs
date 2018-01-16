using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Create.Binding
{
    public abstract class BindableBase<T, Y> : MonoBehaviour, IBindable
    {
        public T Target { get; set; }

        /// <summary>
        /// Should the UI value only be applied if the UI element is selected? This should be enabled for sliders and input fields etc, since otherwise it is not possible to bind several elements
        /// to the same target property without interference. However, elements such as toggle groups have no single element that affects the target value, so checking for selection would not yield correct
        /// results there.
        /// </summary>
        protected bool _requireUIElementSelection = true;

        protected InputField _inputField;
        protected Slider _slider;
        protected Toggle _toggle;
        protected Text _text;
        private Y _previousTargetValue;
        private bool _wasValueInited;
        private ICustomInput<Y> _customInput;

        protected abstract void SetTargetValue(Y value);
        protected abstract Y GetTargetValue();

        protected virtual void Awake()
        {
            _inputField = GetComponent<InputField>();
            _slider = GetComponent<Slider>();
            _toggle = GetComponent<Toggle>();
            _text = GetComponent<Text>();
            _customInput = GetComponent<ICustomInput<Y>>();
        }

        protected virtual void Start()
        {
            if (Target == null)
            {
                Debug.LogWarningFormat("Target must be set in {0} {1}.", GetType(), gameObject.name);
            }
        }

        void LateUpdate()
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
            if (!AreValuesEqual(uiValue, targetValue))
            {
                //If the UI value changed more recently, push the UI value to the target.
                if (DidUIChangeMoreRecently(targetValue))
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


        public void SetTarget(object target)
        {
            Target = (T)target;
        }

        private bool DidUIChangeMoreRecently(Y targetValue)
        {
            if (_customInput == null || _customInput != null && !_requireUIElementSelection)
            {
                return AreValuesEqual(targetValue, _previousTargetValue) && IsUIElementSelected();
            }
            // On custom unputs, don't compare target values, just always push UI value when UI is flagged as selected.
            else
            {
                return IsUIElementSelected();
            }
        }

        private bool AreValuesEqual(Y one, Y two)
        {
            if (one is float)
            {
                return Mathf.Approximately((float)Convert.ChangeType(one, typeof(float)), (float)Convert.ChangeType(two, typeof(float)));
            }

            return two.Equals(one);
        }

        private bool IsUIElementSelected()
        {
            if (!_requireUIElementSelection)
            {
                return true;
            }
            else if (_customInput != null)
            {
                return _customInput.IsSelected;
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

        protected virtual Y GetUIValue()
        {
            Y uiValue = default(Y);
            try
            {
                //Convert the ui element value to the target type.
                if (_inputField != null)
                {
                    uiValue = (Y)Convert.ChangeType(_inputField.text, typeof(Y));
                }
                else if (_slider != null)
                {
                    uiValue = (Y)Convert.ChangeType(_slider.value, typeof(Y));
                }
                else if (_toggle != null)
                {
                    uiValue = (Y)Convert.ChangeType(_toggle.isOn, typeof(Y));
                }
                else if (_customInput != null)
                {
                    uiValue = (Y)Convert.ChangeType(_customInput.Value, typeof(Y));
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
                _inputField.text = targetValue.ToString();
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
                _text.text = targetValue.ToString();
            }
            else if (_customInput != null)
            {
                _customInput.Value = targetValue;
            }
        }
    }
}