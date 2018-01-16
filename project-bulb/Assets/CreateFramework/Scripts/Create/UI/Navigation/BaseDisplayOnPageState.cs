using Create.Controllers;
using Create.Helpers;
using Create.UI.Tweenables;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Create.UI.Navigation
{
    [ExecuteInEditMode]
    public class BaseDisplayOnPageState<T> : ChildPropertyChangeListener<BaseApplicationController<T>> where T : struct
    {
        public bool Invert;
        public T[] VisiblePages = new T[] { default(T) };

        protected TweenableGroup _tweenableGroup;
        protected Tweenable _tweenable;

#if UNITY_EDITOR
        private T _previousInEditorPage;
#endif

        protected override void OnEnable()
        {
            _tweenable = GetComponent<Tweenable>();
            _tweenableGroup = GetComponent<TweenableGroup>();

            if (_tweenableGroup == null && _tweenable == null)
            {
                Debug.LogWarningFormat("[{0}] A {1} or {2} must be present on {3}.", GetType(), typeof(TweenableGroup), typeof(Tweenable), name);
                return;
            }

            base.OnEnable();
            UpdateVisibility();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor time visibility update.
        /// </summary>
        void Update()
        {
            if (Application.isPlaying || _provider == null)
                return;

            if (!_previousInEditorPage.Equals(_provider.InEditorVisiblePage))
            {
                UpdateVisibility();
                _previousInEditorPage = _provider.InEditorVisiblePage;
            }
        }
#endif

        protected override void Provider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateVisibility();
        }

        protected virtual void UpdateVisibility()
        {
            if (VisiblePages == null)
                return;

            bool toVisible = VisiblePages.Contains(_provider.CurrentPage);

            if (_tweenableGroup != null)
            {
                GetComponent<TweenableGroup>().TweenToState(Invert ? !toVisible : toVisible);
            }
            else if (_tweenable != null)
            {
                GetComponent<Tweenable>().TweenToState(Invert ? !toVisible : toVisible);
            }
        }
    }
}