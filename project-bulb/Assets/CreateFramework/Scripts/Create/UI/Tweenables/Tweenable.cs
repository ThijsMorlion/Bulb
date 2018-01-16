using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.Events;
using System;
using System.ComponentModel;

namespace Create.UI.Tweenables
{
    /// <summary>
    /// Utility to easily set the in and out state of a game object, and define tweening properties.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Tweenable : TweenableBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [HideInInspector]
        public float Duration = 0.25f;
        public float Delay;
        public DelayModes DelayMode;
        public EaseType EaseType = EaseType.InOut;
        public bool AnimateTransform = false;
        [HideInInspector]
        public MovementMethod MovementMethod;
        [HideInInspector]
        public float SinusodalScale = 1;
        [HideInInspector]
        public MovementDurationType MovementDurationType;
        /// <summary>
        /// If the movement duration type is set to distance, this along with the traveled distance is used to calculate the duration of the animation.
        /// </summary>
        [HideInInspector]
        public float UnitsPerSecond = 100, MinDuration, MaxDuration;
        public bool AnimateColor = false;

        [Tooltip("Should the object start in its out state?")]
        public bool StartInOutState = true;

        public TweenableEvent OnStarted, OnCompleted;

        public bool IsControlledByTweenableGroup { get; private set; }

        [HideInInspector]
        public bool NoDelayMirrorInTweenableGroup, IgnoreInTweenableGroup;

        private TweenableState _state;
        public TweenableState State
        {
            get { return _state; }
            private set
            {
                if (_state == value)
                    return;
                _state = value;
                RaisePropertyChanged("State");
            }
        }

        [SerializeField, HideInInspector]
        private Vector2 _inAnchorPos, _outAnchorPos, _inSizeDelta, _outSizeDelta, _inOffsetMin, _inOffsetMax, _outOffsetMin, _outOffsetMax;
        [SerializeField, HideInInspector]
        private Vector3 _inLocalPos, _outLocalPos, _inScale, _outScale;
        [SerializeField, HideInInspector]
        private Quaternion _inLocalRotation, _outLocalRotation;
        [SerializeField, HideInInspector]
        private Color _inColor, _outColor;

        private IEnumerator _animateRoutine;
        private RectTransform _rect;
        private Graphic _graphic;
        private TextMeshProUGUI _tmpText;
        private Vector2 _startAnchor, _startSizeDelta, _startOffsetMin, _startOffsetMax;
        private Vector3 _startPosition, _startScale;
        private Quaternion _startRotation;
        private Color _startColor;
        private float _erpPos;

        private TweenableBase[] _tweenables;

        void OnEnable()
        {
            Init();

            // Init in and out states by default.
            if (!_inStateSet && !_outStateSet)
            {
                SetIn();
                SetOut();
            }
        }

        void Awake()
        {
            Init();

            if (StartInOutState)
            {
                if (!_outStateSet)
                {
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning("Trying to start in out state, but no out state is set.");
                    }
                }
                else
                {
                    Snap(false);
                }
            }
            else
            {
                if (!_inStateSet)
                {
                    SetIn();
                    State = TweenableState.In;
                }

                Snap(true);
            }
        }

        void OnValidate()
        {
            if (Duration < 0)
                Duration = 0;
            if (Delay < 0)
                Delay = 0;
        }

        public void TweenToState(bool toIn)
        {
            if (toIn && !_inStateSet)
            {
                Debug.LogWarning("Trying to move to in state, but the state has not been defined.");
                return;
            }
            else if (!toIn && !_outStateSet)
            {
                Debug.LogWarning("Trying to move to out state, but the state has not been defined.");
                return;
            }

            if (toIn && (State == TweenableState.In || State == TweenableState.BetweenToIn) || !toIn && (State == TweenableState.Out || State == TweenableState.BetweenToOut))
                return;

            if (Application.isPlaying)
            {
                // Stop any previous tweens.
                if (_animateRoutine != null)
                {
                    StopCoroutine(_animateRoutine);
                }

                // Start the animation.
                State = toIn ? TweenableState.BetweenToIn : TweenableState.BetweenToOut;
                // 1 - x interpolation time if the previous animation was interrupted.
                if (_erpPos != 0)
                {
                    _erpPos = 1 - _erpPos;
                }

                _animateRoutine = RunAnimateProperties(toIn);
                StartCoroutine(_animateRoutine);
                if (OnStarted != null)
                {
                    OnStarted.Invoke(toIn);
                }
            }
            else
            {
                // Snap in the editor. Potentially animate in the editor in the future, using EditorApplication.update.
                Snap(toIn);
            }
        }

        public override void Snap(bool toIn)
        {
            if (_animateRoutine != null)
            {
                StopCoroutine(_animateRoutine);
            }

            base.Snap(toIn);

            _erpPos = 0;
            State = toIn ? TweenableState.In : TweenableState.Out;
            if (OnCompleted != null)
            {
                OnCompleted.Invoke(toIn);
            }
        }

        /// <summary>
        /// Helper for runtime Tweenable animations.
        /// </summary>
        public void SetLocalInOutPositions(Vector3 inLocal, Vector3 outLocal)
        {
            if (_rect == null)
            {
                _inLocalPos = inLocal;
                _outLocalPos = outLocal;
            }
            else
            {
                _inAnchorPos = inLocal;
                _outLocalPos = outLocal;
            }
        }

        public override void SetInterpolation(float erpPos, bool toIn)
        {
            // Animate transform.
            if (AnimateTransform)
            {
                InterpolateTransform(erpPos, toIn);
            }

            // Animate image color and alpha.
            if (AnimateColor)
            {
                InterpolateColor(erpPos, toIn);
            }

            // Set all other tweenables.
            if (_tweenables == null)
            {
                _tweenables = GetOtherTweenables();
            }
            foreach (var tweenable in _tweenables)
            {
                tweenable.SetInterpolation(erpPos, toIn);
            }
        }

        public override void SetIn()
        {
            _inStateSet = true;

            if (_rect == null)
                _rect = GetComponent<RectTransform>();
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();

            // Set transform.
            if (_rect != null)
            {
                _inAnchorPos = _rect.anchoredPosition;
                _inSizeDelta = _rect.sizeDelta;
                _inLocalRotation = _rect.localRotation;
                _inLocalPos = _rect.localPosition;
                _inOffsetMin = _rect.offsetMin;
                _inOffsetMax = _rect.offsetMax;
            }
            else
            {
                _inLocalPos = transform.localPosition;
                _inLocalRotation = transform.localRotation;
            }
            _inScale = transform.localScale;

            // Set color.
            if (_tmpText != null)
            {
                _inColor = _tmpText.color;
            }
            else if (_graphic != null)
            {
                _inColor = _graphic.color;
            }

            // Set all other tweenables.
            if (_tweenables == null)
            {
                _tweenables = GetOtherTweenables();
            }
            foreach (var tweenable in _tweenables)
            {
                tweenable.SetIn();
            }
        }

        public override void SetOut()
        {
            _outStateSet = true;

            if (_rect == null)
                _rect = GetComponent<RectTransform>();
            if (_graphic == null)
                _graphic = GetComponent<Graphic>();

            // Set transform.
            if (_rect != null)
            {
                _outAnchorPos = _rect.anchoredPosition;
                _outSizeDelta = _rect.sizeDelta;
                _outLocalRotation = _rect.localRotation;
                _outLocalPos = _rect.localPosition;
                _outOffsetMin = _rect.offsetMin;
                _outOffsetMax = _rect.offsetMax;
            }
            else
            {
                _outLocalPos = transform.localPosition;
                _outLocalRotation = transform.localRotation;
            }
            _outScale = transform.localScale;

            // Set color.
            if (_tmpText != null)
            {
                _outColor = _tmpText.color;
            }
            else if (_graphic != null)
            {
                _outColor = _graphic.color;
            }

            // Set all other tweenables.
            if (_tweenables == null)
            {
                _tweenables = GetOtherTweenables();
            }
            foreach (var tweenable in _tweenables)
            {
                tweenable.SetOut();
            }
        }

        public override void GetStartingValues(bool toIn)
        {
            // Get own starting values, and for all other tweenables.
            if (_tweenables == null)
            {
                _tweenables = GetOtherTweenables();
            }
            foreach (var tweenable in _tweenables)
            {
                tweenable.GetStartingValues(toIn);
            }

            if (_rect != null)
            {
                _startAnchor = toIn ? _outAnchorPos : _inAnchorPos;
                _startSizeDelta = toIn ? _outSizeDelta : _inSizeDelta;
                _startOffsetMin = toIn ? _outOffsetMin : _inOffsetMin;
                _startOffsetMax = toIn ? _outOffsetMax : _inOffsetMax;
            }
            _startPosition = toIn ? _outLocalPos : _inLocalPos;
            _startScale = toIn ? _outScale : _inScale;
            _startRotation = toIn ? _outLocalRotation : _inLocalRotation;

            if (_graphic != null)
            {
                _startColor = toIn ? _outColor : _inColor;
            }
        }

        private void Init()
        {
            _tweenables = GetOtherTweenables();
            if (_rect == null)
                _rect = GetComponent<RectTransform>();
            if (_tmpText == null)
                _tmpText = GetComponent<TextMeshProUGUI>();
            if (_tmpText == null && _graphic == null)
                _graphic = GetComponent<Graphic>();

            IsControlledByTweenableGroup = GetComponentInParent<TweenableGroup>() != null;
        }

        private void InterpolateTransform(float erpPos, bool toIn)
        {
            if (_rect != null)
            {
                if (MovementMethod == MovementMethod.OffsetsOnly)
                {
                    _rect.offsetMin = Vector2.Lerp(_startOffsetMin, toIn ? _inOffsetMin : _outOffsetMin, erpPos);
                    _rect.offsetMax = Vector2.Lerp(_startOffsetMax, toIn ? _inOffsetMax : _outOffsetMax, erpPos);
                }
                else
                {
                    _rect.sizeDelta = Vector2.Lerp(_startSizeDelta, toIn ? _inSizeDelta : _outSizeDelta, erpPos);

                    if (MovementMethod == MovementMethod.Straight)
                    {
                        _rect.anchoredPosition = Vector2.Lerp(_startAnchor, toIn ? _inAnchorPos : _outAnchorPos, erpPos);
                    }
                    else if (MovementMethod == MovementMethod.Sinusodal)
                    {
                        _rect.anchoredPosition = SinusodalLerp(_startAnchor, toIn ? _inAnchorPos : _outAnchorPos, erpPos);
                    }
                }
            }
            else
            {
                if (MovementMethod == MovementMethod.Straight)
                {
                    transform.localPosition = Vector3.Lerp(_startPosition, toIn ? _inLocalPos : _outLocalPos, erpPos);
                }
                else if (MovementMethod == MovementMethod.Sinusodal)
                {
                    transform.localPosition = SinusodalLerp(_startPosition, toIn ? _inLocalPos : _outLocalPos, erpPos);
                }
            }

            if (_inLocalRotation != _outLocalRotation)
            {
                transform.localRotation = Quaternion.Lerp(_startRotation, toIn ? _inLocalRotation : _outLocalRotation, erpPos);
            }
            if (_inScale != _outScale)
            {
                transform.localScale = Vector3.Lerp(_startScale, toIn ? _inScale : _outScale, erpPos);
            }
        }

        private void InterpolateColor(float erpPos, bool toIn)
        {
            if (_inColor != _outColor)
            {
                Color lerpedColor = Color.Lerp(_startColor, toIn ? _inColor : _outColor, erpPos);

                // Even though TextMeshPro text inherits from graphic, setting the .color as graphic does not change the text's color.
                if (_tmpText != null)
                {
                    _tmpText.color = lerpedColor;
                }
                else if (_graphic != null)
                {
                    _graphic.color = lerpedColor;
                }
            }
        }

        private Vector2 SinusodalLerp(Vector2 from, Vector2 to, float erpPos)
        {
            Vector2 delta = to - from;
            float xFactor = 1;
            if (delta.y != 0)
            {
                xFactor = Mathf.Abs(delta.x / delta.y);
                xFactor = Mathf.Clamp01(xFactor);
                xFactor = 1f - xFactor;
            }

            float sin = Mathf.Sin(erpPos * Mathf.PI) * SinusodalScale;
            return new Vector2(Mathf.Lerp(from.x, to.x, erpPos) + sin * xFactor, Mathf.Lerp(from.y, to.y, erpPos) + sin * (1f - xFactor));
        }

        private Vector3 SinusodalLerp(Vector3 from, Vector3 to, float erpPos)
        {
            Vector2 delta = to - from;
            float xFactor = 1;
            if (delta.y != 0)
            {
                xFactor = Mathf.Abs(delta.x / delta.y);
                xFactor = Mathf.Clamp01(xFactor);
                xFactor = 1f - xFactor;
            }

            float sin = Mathf.Sin(erpPos * Mathf.PI) * SinusodalScale;
            return new Vector3(Mathf.Lerp(from.x, to.x, erpPos) + sin * xFactor, Mathf.Lerp(from.y, to.y, erpPos) + sin * (1f - xFactor), Mathf.Lerp(from.z, to.z, erpPos) + sin);
        }

        private TweenableBase[] GetOtherTweenables()
        {
            return GetComponents<TweenableBase>().Where(t => t != this).ToArray();
        }

        private IEnumerator RunAnimateProperties(bool toIn)
        {
            // Wait for the delay if the delay isn't being controlled by a tweenable group, and if the delay mode criterium is met.
            if ((IgnoreInTweenableGroup || !IsControlledByTweenableGroup) && Delay > 0 && (DelayMode == DelayModes.Both || DelayMode == DelayModes.InOnly && toIn || DelayMode == DelayModes.OutOnly && !toIn))
                yield return new WaitForSecondsRealtime(Delay);

            // Init the values to start the interpolation from.
            GetStartingValues(toIn);

            // Determine duratiom.
            float duration = CalculateDuration();

            // Run the animation on all tweenables.
            while (_erpPos < 1)
            {
                SetInterpolation(ApplyEasing(_erpPos), toIn);
                _erpPos = Mathf.MoveTowards(_erpPos, 1, 1f / duration * Time.unscaledDeltaTime);
                yield return null;
            }

            // Finish at 1.
            SetInterpolation(1, toIn);
            _erpPos = 1;
            // Update the state.
            State = toIn ? TweenableState.In : TweenableState.Out;
            // Alert listeners that the animation completed.
            if (OnCompleted != null)
            {
                OnCompleted.Invoke(toIn);
            }
        }

        private float CalculateDuration()
        {
            // Animation duration by distance traveled.
            if (AnimateTransform && MovementDurationType == MovementDurationType.Distance)
            {
                float distance = 0;
                if (_inLocalPos != Vector3.zero || _outLocalPos != Vector3.zero)
                {
                    distance = Vector3.Distance(_inLocalPos, _outLocalPos);
                }
                else
                {
                    distance = Vector2.Distance(_inAnchorPos, _outAnchorPos);
                }

                float duration = distance / UnitsPerSecond;
                if (MinDuration != 0 || MaxDuration != 0)
                {
                    duration = Mathf.Clamp(duration, MinDuration, MaxDuration);
                }
                return duration;
            }

            // Regular duration.
            return Duration;
        }

        private float ApplyEasing(float erpPos)
        {
            switch (EaseType)
            {
                case EaseType.In:
                    return Mathfx.Coserp(0, 1, erpPos);
                case EaseType.Out:
                    return Mathfx.Sinerp(0, 1, erpPos);
                case EaseType.InOut:
                    return Mathfx.Hermite(0, 1, erpPos);
                case EaseType.Boing:
                    return Mathfx.Berp(0, 1, erpPos, .5f);
                case EaseType.Bounce:
                    return Mathfx.Bounce(erpPos);
            }

            return erpPos;
        }

        private void RaisePropertyChanged(string v)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(v));
        }
    }

    public enum EaseType
    {
        Linear,
        InOut,
        Out,
        In,
        Boing,
        Bounce
    }

    public enum MovementMethod
    {
        Straight,
        Sinusodal,
        OffsetsOnly
    }

    public enum MovementDurationType
    {
        Time,
        Distance
    }

    public enum TweenableState
    {
        Out,
        In,
        BetweenToIn,
        BetweenToOut
    }

    public enum DelayModes
    {
        Both,
        InOnly,
        OutOnly
    }

    [Serializable]
    public class TweenableEvent : UnityEvent<bool> { }
}