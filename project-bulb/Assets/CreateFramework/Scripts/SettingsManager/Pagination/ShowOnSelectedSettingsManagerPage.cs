using UnityEngine;
using Create.UI.Tweenables;

namespace Settings.Pagination
{
    [RequireComponent(typeof(TweenableCanvasGroup))]
    public class ShowOnSelectedSettingsManagerPage : MonoBehaviour
    {
        public SettingsManagerPagesController.SettingsManagerPages ThisSettingsManagerPage;

        private SettingsManagerPagesController _settingsManagerPageController;
        private SettingsManagerVisibility _settingsManagerVisibility;
        private Tweenable _tweenable;

        private void OnEnable()
        {
            _settingsManagerPageController = GetComponentInParent<SettingsManagerPagesController>();
            _settingsManagerPageController.PropertyChanged += SettingsManagerPageController_PropertyChanged;

            _settingsManagerVisibility = GetComponentInParent<SettingsManagerVisibility>();
            _settingsManagerVisibility.PropertyChanged += SettingsManagerVisibility_PropertyChanged;

            _tweenable = GetComponent<Tweenable>();
        }

        private void OnDisable()
        {
            _settingsManagerPageController.PropertyChanged -= SettingsManagerPageController_PropertyChanged;
            _settingsManagerVisibility.PropertyChanged -= SettingsManagerVisibility_PropertyChanged;
        }

        private void SettingsManagerVisibility_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSettingsManagerVisible")
            {
                //hide when the settingmanager is completly hidden
                if (!_settingsManagerVisibility.IsSettingsManagerVisible)
                {
                    _tweenable.TweenToState(false);
                }
            }
        }

        private void SettingsManagerPageController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == GetPropertyName(() => _settingsManagerPageController.SelectedPage))
            {
                _tweenable.TweenToState(_settingsManagerPageController.SelectedPage == ThisSettingsManagerPage);
            }
        }

        public string GetPropertyName<T>(System.Linq.Expressions.Expression<System.Func<T>> propertyLambda)
        {
            var body = propertyLambda.Body as System.Linq.Expressions.MemberExpression;
            if (body == null)
            {
                throw new System.ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }
            return body.Member.Name;
        }
    }
}