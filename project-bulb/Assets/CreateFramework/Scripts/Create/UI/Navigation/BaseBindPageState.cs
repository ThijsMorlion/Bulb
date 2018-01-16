using Create.Binding;
using Create.Controllers;
using System;
using UnityEngine.UI;

namespace Create.UI.Navigation
{
    public class BaseBindPageState<T> : BindableBase<Toggle, bool> where T : struct, IConvertible
    {
        public T TargetPage;

        private BaseApplicationController<T> _appController;

        protected override void Start()
        {
            Target = GetComponent<Toggle>();
            base.Start();
            _appController = GetComponentInParent<BaseApplicationController<T>>();
            _requireUIElementSelection = false;
        }

        protected override bool GetTargetValue()
        {
            if (_appController == null)
                return false;

            return _appController.CurrentPage.Equals(TargetPage);
        }

        protected override void SetTargetValue(bool value)
        {
            if (Target == null)
                return;

            Target.isOn = value;
            if (value && _appController != null)
            {
                _appController.CurrentPage = TargetPage;
            }
        }
    }
}