using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Create.UI.Tweenables
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(Tweenable)), RequireComponent(typeof(CanvasGroup))]
    public class TweenableCanvasGroup : TweenableBase
    {
        public bool DisableInteractivity = true;
        public bool IgnoreInLayout;
        /// <summary>
        /// If set, scripts marked with the DisableInCanvasGroup attribute will be disabled when the tweenable canvas group is fully out.
        /// </summary>
        [Tooltip("If set, scripts marked with the DisableInCanvasGroup attribute will be disabled when the tweenable canvas group is fully out.")]
        public bool DisableScripts = true;

        [SerializeField, HideInInspector]
        private float _inAlpha, _outAlpha;
        private float _startAlpha;

        private CanvasGroup _canvasGroup;
        private LayoutElement _layoutElement;
        private MonoBehaviour[] _scriptsToDisable;

        void OnEnable()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_layoutElement == null)
                _layoutElement = GetComponent<LayoutElement>();
            _scriptsToDisable = GetComponentsInChildren<MonoBehaviour>().Where(s => Attribute.GetCustomAttribute(s.GetType(), typeof(DisableInCanvasGroupAttribute)) != null).ToArray();

            // Apply default 1 / 0 alpha by default.
            if (!_inStateSet && !_outStateSet)
            {
                _inAlpha = 1;
                _outAlpha = 0;
                _inStateSet = _outStateSet = true;
            }
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                _scriptsToDisable = GetComponentsInChildren<MonoBehaviour>().Where(s => Attribute.GetCustomAttribute(s.GetType(), typeof(DisableInCanvasGroupAttribute)) != null).ToArray();
            }

            _canvasGroup.alpha = Mathf.Lerp(_startAlpha, toIn ? _inAlpha : _outAlpha, erpPos);

            // Enable canvas group interactivity only once it is fully visible.
            if (DisableInteractivity && erpPos == 1 && toIn)
            {
                _canvasGroup.blocksRaycasts = true;
            }
            // Make the canvas group pass through as soon as any move towards the out state has been made.
            else if (DisableInteractivity && !toIn)
            {
                _canvasGroup.blocksRaycasts = false;
            }

            UpdateIgnoreLayoutState(erpPos, toIn);
            UpdateScriptsEnabledState(erpPos, toIn);
        }

        private void UpdateIgnoreLayoutState(float erpPos, bool toIn)
        {
            if (IgnoreInLayout && _layoutElement != null)
            {
                // Immediately add the element back to the layout when interpolating inwards.
                if (toIn)
                {
                    _layoutElement.ignoreLayout = false;
                }
                // Ignore in layout once fully hidden.
                else if (!toIn && erpPos == 1)
                {
                    _layoutElement.ignoreLayout = true;
                }
            }
        }

        private void UpdateScriptsEnabledState(float erpPos, bool toIn)
        {
            if (Application.isPlaying && DisableScripts && _scriptsToDisable != null)
            {
                foreach (var script in _scriptsToDisable)
                {
                    // Immediately add the element back to the layout when interpolating inwards.
                    if (toIn && !script.enabled)
                    {
                        script.enabled = true;
                    }
                    // Ignore in layout once fully hidden.
                    else if (!toIn && erpPos == 1 && script.enabled)
                    {
                        script.enabled = false;
                    }
                }
            }
        }

        public override void SetIn()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _inStateSet = true;
            _inAlpha = _canvasGroup.alpha;
        }

        public override void SetOut()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _outStateSet = true;
            _outAlpha = _canvasGroup.alpha;
        }

        public override void GetStartingValues(bool toIn)
        {
            _startAlpha = toIn ? _outAlpha : _inAlpha;
        }
    }

    public class DisableInCanvasGroupAttribute : Attribute { }
}