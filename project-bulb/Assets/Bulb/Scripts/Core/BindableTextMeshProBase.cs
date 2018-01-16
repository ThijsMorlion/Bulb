using System;
using Create.Binding;
using TMPro;

namespace Bulb.Core
{
    public abstract class BindableTextMeshProBase<T, Y> : BindableBase<T, Y>
    {
        protected TMP_InputField _tmProInputField;
        protected TMP_Text _tmProText;

        protected override void Awake()
        {
            _tmProInputField = GetComponent<TMP_InputField>();
            _tmProText = GetComponent<TMP_Text>();

            base.Awake();
        }

        protected override Y GetUIValue()
        {
            var uiValue = base.GetUIValue();

            if (uiValue == null || uiValue.Equals(default(Y)))
            {
                if (_tmProInputField != null)
                {
                    uiValue = (Y)Convert.ChangeType(_tmProInputField.text, typeof(Y));
                }
                else if (_tmProText != null)
                {
                    uiValue = (Y)Convert.ChangeType(_tmProText.text, typeof(Y));
                }
            }

            return uiValue;
        }

        protected override void SetUIValue(Y targetValue)
        {
            if (_tmProInputField != null)
            {
                _tmProInputField.text = targetValue.ToString();
            }
            else if (_tmProText != null)
            {
                _tmProText.text = targetValue.ToString();
            }
            else
            {
                base.SetUIValue(targetValue);
            }
        }
    }
}
