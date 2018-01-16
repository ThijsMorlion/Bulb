using System;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

namespace Create.UI.Tweenerts
{
    [ExecuteInEditMode]
    public class Tweenert : MonoBehaviour
    {
        public AnimationCurves AnimationCurve;
        public bool IgnoreInGroup;
        public List<State> States = new List<State>(0);
        public List<string> StatesInParents = new List<string>(0);

        private RectTransform _rectt;
        private CanvasGroup _canvasGroup;
        private TweenertGroup _tweenertGroup;
        private IEnumerator _animationRoutine;

        private List<TransformComponents> _componentsToAnimate;
        private Vector3 _fromAnchoredPosition;
        private Vector3 _toAnchoredPosition;
        private Vector3 _fromLocalScale;
        private Vector3 _toLocalScale;
        private Vector2 _fromSizeDelta;
        private Vector2 _toSizeDelta;
        private float _fromRotation;
        private float _toRotation;
        private float _fromAlpha;
        private float _toAlpha;

        [Serializable]
        public class State
        {
            public string StateName;

            public bool AnimateAnchoredPosition;
            public Vector3 AnchoredPosition;

            public bool AnimateLocalScale;
            public Vector3 LocalScale;

            public bool AnimateSizeDelta;
            public Vector2 SizeDelta;

            public bool AnimateRotation;
            public float Rotation;

            public bool AnimateAlpha;
            public float Alpha;
        }

        public enum AnimationCurves
        {
            Linear,
            EaseInOut,
            Elastic,
            Bounce,
        }

        public enum TransformComponents
        {
            anchoredPosition,
            sizeDelta,
            rotation,
            localScale,
            alpha,
        }

        private void OnEnable()
        {
            _rectt = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            _tweenertGroup = GetComponent<TweenertGroup>();
            if (_tweenertGroup == null)
            {
                _tweenertGroup = GetComponentInParent<TweenertGroup>();
            }

            if (_tweenertGroup != null)
            {
                _tweenertGroup.PropertyChanged += AnimateTransformGroup_PropertyChanged;
            }
        }

        private void OnDisable()
        {
            if (_tweenertGroup != null)
            {
                _tweenertGroup.PropertyChanged -= AnimateTransformGroup_PropertyChanged;
            }
        }

        private void Update()
        {
            FillStatesInParentsList();
        }

        private void AnimateTransformGroup_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //when the state of the parent transformgroup changed
            if (e.PropertyName == GetPropertyName(() => _tweenertGroup.State))
            {
                if (!IgnoreInGroup)
                {
                    if (_tweenertGroup.Duration == 0)
                    {
                        SnapToState(_tweenertGroup.State);
                    }
                    else
                    {
                        AnimateToState(_tweenertGroup.State, _tweenertGroup.Duration);
                    }
                }
            }
        }

        private void FillStatesInParentsList()
        {
            StatesInParents = new List<string>(0);
            foreach (var animateTransform in GetComponentsInParent<Tweenert>())
            {
                foreach (var state in animateTransform.States)
                {
                    if (!StatesInParents.Contains(state.StateName))
                    {
                        StatesInParents.Add(state.StateName);
                    }
                }
            }
        }

        //animate to state
        public void AnimateToState(string stateName, float duration)
        {
            State toState = States.Find(o => o.StateName == stateName);

            if (toState != null)
            {
                if (toState.AnimateAnchoredPosition)
                {
                    AnimatePosition(_rectt.anchoredPosition, toState.AnchoredPosition, duration);
                }

                if (toState.AnimateLocalScale)
                {
                    AnimateLocalScale(_rectt.localScale, toState.LocalScale, duration);
                }

                if (toState.AnimateSizeDelta)
                {
                    AnimateSizeDelta(_rectt.sizeDelta, toState.SizeDelta, duration);
                }

                if (toState.AnimateRotation)
                {
                    AnimateRotation(transform.eulerAngles.z, toState.Rotation, duration);
                }

                if (toState.AnimateAlpha)
                {
                    if (_canvasGroup != null)
                    {
                        AnimateAlpha(_canvasGroup.alpha, toState.Alpha, duration);
                    }
                }
            }
        }

        public void SnapToState(string stateName)
        {
            State toState = States.Find(o => o.StateName == stateName);

            if (toState != null)
            {
                if (toState.AnimateAnchoredPosition)
                {
                    _rectt.anchoredPosition = toState.AnchoredPosition;
                }

                if (toState.AnimateLocalScale)
                {
                    _rectt.localScale = toState.LocalScale;
                }

                if (toState.AnimateSizeDelta)
                {
                    _rectt.sizeDelta = toState.SizeDelta;
                }

                if (toState.AnimateRotation)
                {
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, toState.Rotation);
                }

                if (toState.AnimateAlpha)
                {
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = toState.Alpha;
                    }
                }
            }
        }

        //snaps rectt to values of component from statesitem, method called from custom editor
        public void SnapToStateListItemComponentValues(TransformComponents component, int i)
        {
            if (component == TransformComponents.anchoredPosition)
            {
                _rectt.anchoredPosition = States[i].AnchoredPosition;
            }

            if (component == TransformComponents.localScale)
            {
                _rectt.localScale = States[i].LocalScale;
            }

            if (component == TransformComponents.sizeDelta)
            {
                _rectt.sizeDelta = States[i].SizeDelta;
            }

            if (component == TransformComponents.rotation)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, States[i].Rotation);
            }

            if (component == TransformComponents.alpha)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = States[i].Alpha;
                }
            }
        }

        //sets the name of a state at a specified index
        public void ChangeStateName(int index, string stateName)
        {
            States[index].StateName = stateName;
        }

        //copies current rectt values to component of statesitem, method called from custom editor
        public void CopyCurrentComponentValuesToStateListItem(TransformComponents component, int i)
        {
            if (component == TransformComponents.anchoredPosition)
            {
                States[i].AnchoredPosition = _rectt.anchoredPosition;
            }

            if (component == TransformComponents.localScale)
            {
                States[i].LocalScale = _rectt.localScale;
            }

            if (component == TransformComponents.sizeDelta)
            {
                States[i].SizeDelta = _rectt.sizeDelta;
            }

            if (component == TransformComponents.rotation)
            {
                States[i].Rotation = transform.eulerAngles.z;
            }

            if (component == TransformComponents.alpha)
            {
                if (_canvasGroup != null)
                {
                    States[i].Alpha = _canvasGroup.alpha;
                }
            }
        }

        //manually animate position
        public void AnimatePosition(Vector2 fromPosition, Vector2 toPosition, float duration)
        {
            PrepareNewAnimation();
            AddComponentToAnimateToList(TransformComponents.anchoredPosition);

            _fromAnchoredPosition = fromPosition;
            _toAnchoredPosition = toPosition;

            _animationRoutine = RunAnimationRoutine(duration);
            StartCoroutine(_animationRoutine);
        }

        //manually animate size
        public void AnimateSizeDelta(Vector2 fromSizeDelta, Vector2 toSizeDelta, float duration)
        {
            PrepareNewAnimation();
            AddComponentToAnimateToList(TransformComponents.sizeDelta);

            _fromSizeDelta = fromSizeDelta;
            _toSizeDelta = toSizeDelta;

            _animationRoutine = RunAnimationRoutine(duration);
            StartCoroutine(_animationRoutine);
        }

        //manually animate rotation
        public void AnimateRotation(float fromRotation, float toRotation, float duration)
        {
            PrepareNewAnimation();
            AddComponentToAnimateToList(TransformComponents.rotation);

            _fromRotation = fromRotation;
            _toRotation = toRotation;

            _animationRoutine = RunAnimationRoutine(duration);
            StartCoroutine(_animationRoutine);
        }

        //manually animate scale
        public void AnimateLocalScale(Vector3 fromScale, Vector3 toScale, float duration)
        {
            PrepareNewAnimation();
            AddComponentToAnimateToList(TransformComponents.localScale);

            _fromLocalScale = fromScale;
            _toLocalScale = toScale;

            _animationRoutine = RunAnimationRoutine(duration);
            StartCoroutine(_animationRoutine);
        }

        //manually animate alpha
        public void AnimateAlpha(float fromAlpha, float toAlpha, float duration)
        {
            PrepareNewAnimation();
            AddComponentToAnimateToList(TransformComponents.alpha);

            _fromAlpha = fromAlpha;
            _toAlpha = toAlpha;

            _animationRoutine = RunAnimationRoutine(duration);
            StartCoroutine(_animationRoutine);
        }

        //manually animate all components
        public void AnimateAll(Vector2 fromPosition, float fromRotation, Vector3 fromScale, Vector2 fromSizeDelta, float fromAlpha, Vector2 toPosition, float toRotation, Vector3 toScale, Vector2 toSizeDelta, float toAlpha, float duration)
        {
            PrepareNewAnimation();

            AddComponentToAnimateToList(TransformComponents.anchoredPosition);
            AddComponentToAnimateToList(TransformComponents.rotation);
            AddComponentToAnimateToList(TransformComponents.localScale);
            AddComponentToAnimateToList(TransformComponents.sizeDelta);
            AddComponentToAnimateToList(TransformComponents.alpha);

            _fromAnchoredPosition = fromPosition;
            _toAnchoredPosition = toPosition;
            _fromLocalScale = fromScale;
            _toLocalScale = toScale;
            _fromSizeDelta = fromSizeDelta;
            _toSizeDelta = toSizeDelta;
            _fromRotation = fromRotation;
            _toRotation = toRotation;
            _fromAlpha = fromAlpha;
            _toAlpha = toAlpha;

            _animationRoutine = RunAnimationRoutine(duration);
            StartCoroutine(_animationRoutine);
        }

        public void CancelAnimation()
        {
            StopRunningAnimation();
        }

        private void PrepareNewAnimation()
        {
            if (_rectt == null)
            {
                _rectt = GetComponent<RectTransform>();
            }

            StopRunningAnimation();
        }

        private void StopRunningAnimation()
        {
            if (_animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
            }
        }

        private void AddComponentToAnimateToList(TransformComponents component)
        {
            if (_componentsToAnimate == null)
            {
                _componentsToAnimate = new List<TransformComponents>();
            }

            if (!_componentsToAnimate.Contains(component))
            {
                _componentsToAnimate.Add(component);
            }
        }

        private void RemoveComponentToAnimateFromList(TransformComponents component)
        {
            if (_componentsToAnimate == null)
            {
                _componentsToAnimate = new List<TransformComponents>();
            }

            if (_componentsToAnimate.Contains(component))
            {
                _componentsToAnimate.Remove(component);
            }
        }

        private IEnumerator RunAnimationRoutine(float duration)
        {
            float completionPerc = 0;

            //while the animation is running: set the animated components
            while (completionPerc < 1)
            {
                float animationCurvedCompletionPercentage = AnimationCurveCompletionPerc(completionPerc);

                if (_componentsToAnimate.Contains(TransformComponents.anchoredPosition))
                {
                    _rectt.anchoredPosition = Vector3.Lerp(_fromAnchoredPosition, _toAnchoredPosition, animationCurvedCompletionPercentage);
                }

                if (_componentsToAnimate.Contains(TransformComponents.sizeDelta))
                {
                    _rectt.sizeDelta = Vector2.Lerp(_fromSizeDelta, _toSizeDelta, animationCurvedCompletionPercentage);
                }

                if (_componentsToAnimate.Contains(TransformComponents.localScale))
                {
                    _rectt.localScale = Vector3.Lerp(_fromLocalScale, _toLocalScale, animationCurvedCompletionPercentage);
                }

                if (_componentsToAnimate.Contains(TransformComponents.rotation))
                {
                    transform.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, _fromRotation), Quaternion.Euler(0, 0, _toRotation), animationCurvedCompletionPercentage);
                }

                if (_componentsToAnimate.Contains(TransformComponents.alpha))
                {
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = Mathf.Lerp(_fromAlpha, _toAlpha, animationCurvedCompletionPercentage);
                    }
                }

                completionPerc += (Time.deltaTime / duration);
                yield return null;
            }

            //when the animation has completed: set all used components to their end value
            if (_componentsToAnimate.Contains(TransformComponents.anchoredPosition))
            {
                _rectt.anchoredPosition = _toAnchoredPosition;
                RemoveComponentToAnimateFromList(TransformComponents.anchoredPosition);
            }

            if (_componentsToAnimate.Contains(TransformComponents.sizeDelta))
            {
                _rectt.sizeDelta = _toSizeDelta;
                RemoveComponentToAnimateFromList(TransformComponents.sizeDelta);
            }

            if (_componentsToAnimate.Contains(TransformComponents.localScale))
            {
                _rectt.localScale = _toLocalScale;
                RemoveComponentToAnimateFromList(TransformComponents.localScale);
            }

            if (_componentsToAnimate.Contains(TransformComponents.rotation))
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, _toRotation);
                RemoveComponentToAnimateFromList(TransformComponents.rotation);
            }

            if (_componentsToAnimate.Contains(TransformComponents.alpha))
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = _toAlpha;
                }

                RemoveComponentToAnimateFromList(TransformComponents.alpha);
            }
        }

        private float AnimationCurveCompletionPerc(float origPercentage)
        {
            switch (AnimationCurve)
            {
                case AnimationCurves.Linear:
                    {
                        return Mathfx.Lerp(0, 1, origPercentage);
                    }
                case AnimationCurves.Elastic:
                    {
                        return Mathfx.Berp(0, 1, origPercentage);
                    }
                case AnimationCurves.Bounce:
                    {
                        return Mathfx.Bounce(origPercentage);
                    }
                case AnimationCurves.EaseInOut:
                default:
                    {
                        return Mathfx.Hermite(0, 1, origPercentage);
                    }
            }
        }

        public string GetPropertyName<T>(System.Linq.Expressions.Expression<System.Func<T>> propertyLambda)
        {
            var body = propertyLambda.Body as System.Linq.Expressions.MemberExpression;

            if (body == null)
            {
                throw new ArgumentException("A lambda of the form: '() => Class.Property' or '() => object.Property' must be passsed.");
            }

            return body.Member.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}