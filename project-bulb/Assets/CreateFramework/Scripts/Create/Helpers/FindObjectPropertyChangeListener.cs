using System.ComponentModel;
using UnityEngine;

namespace Create.Helpers
{
    public abstract class FindObjectPropertyChangeListener<T> : MonoBehaviour where T : MonoBehaviour, INotifyPropertyChanged
    {
        protected T _provider;

        protected virtual void OnEnable()
        {
            if (_provider == null)
            {
                _provider = FindObjectOfType<T>();
                if (_provider == null)
                {
                    Debug.LogWarningFormat("[{0}] An object of type {2} was not present in the scene.", GetType(), typeof(T));
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