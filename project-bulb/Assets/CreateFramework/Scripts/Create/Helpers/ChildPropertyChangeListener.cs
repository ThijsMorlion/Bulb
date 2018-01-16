using System.ComponentModel;
using UnityEngine;

namespace Create.Helpers
{
    public abstract class ChildPropertyChangeListener<T> : MonoBehaviour where T : INotifyPropertyChanged
    {
        protected T _provider;

        protected virtual void OnEnable()
        {
            if (_provider == null)
            {
                _provider = GetComponentInParent<T>();
                if (_provider == null)
                {
                    Debug.LogWarningFormat("{0} {1} must be parented to a {2}.", GetType(), name, typeof(T));
                    return;
                }
            }

            _provider.PropertyChanged += Provider_PropertyChanged;
        }

        protected virtual void OnDisable()
        {
            if (_provider != null)
            {
                _provider.PropertyChanged -= Provider_PropertyChanged;
            }
        }

        protected abstract void Provider_PropertyChanged(object sender, PropertyChangedEventArgs e);
    }
}