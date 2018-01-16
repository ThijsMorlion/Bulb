using Create.UI.Tweenables;
using UnityEngine;

namespace Create.Helpers.ImageLoader
{
    [RequireComponent(typeof(Tweenable))]
    public class DisplayOnLoadingImage : MonoBehaviour
    {
        public bool Invert;
        [Tooltip("If set, all image loaders in the root will be or'd. If left blank, only an image loader in its downstream hierarchy will be considered.")]
        public Transform TargetRoot;

        private ILoader[] _loaders;

        void OnEnable()
        {
            if (TargetRoot != null)
            {
                _loaders = TargetRoot.GetComponentsInChildren<ILoader>();
            }
            else
            {
                _loaders = GetComponentsInChildren<ILoader>();
            }

            if (_loaders == null)
            {
                Debug.LogWarningFormat("No image loaders found for DisplayOnLoadingImage {0}.", gameObject.name);
                return;
            }

            foreach (var loader in _loaders)
            {
                loader.PropertyChanged += Loader_PropertyChanged;
            }
        }

        void OnDisable()
        {
            if (_loaders != null)
            {
                foreach (var loader in _loaders)
                {
                    loader.PropertyChanged -= Loader_PropertyChanged;
                }
            }
        }

        private void Loader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsLoading")
            {
                bool isLoading = false;
                foreach (var loader in _loaders)
                {
                    isLoading |= loader.IsLoading;
                }

                GetComponent<Tweenable>().TweenToState(Invert ? !isLoading : isLoading);
            }
        }
    }
}