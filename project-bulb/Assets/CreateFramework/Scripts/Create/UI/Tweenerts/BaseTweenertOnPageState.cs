using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Create.Helpers;
using Create.Controllers;
using System.ComponentModel;
using System.Collections.Generic;

namespace Create.UI.Tweenerts
{
    [Serializable]
    public class PageState
    {
        public int OnPage;
        public bool DoNothing;
        public string StateName;
        public float Duration;

#if UNITY_EDITOR //SerializedProperty requires a reference to UnityEditor which breaks building to standalone builds
        //to cast serializedProperty to PageState
        public static explicit operator PageState(SerializedProperty serializedProperty)
        {
            return new PageState
            {
                DoNothing = serializedProperty.FindPropertyRelative("DoNothing").boolValue,
                Duration = serializedProperty.FindPropertyRelative("Duration").floatValue,
                OnPage = serializedProperty.FindPropertyRelative("OnPage").intValue,
                StateName = serializedProperty.FindPropertyRelative("StateName").stringValue,
            };
        }
#endif
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Tweenert))]
    public class BaseTweenertOnPageState<T> : ChildPropertyChangeListener<BaseApplicationController<T>> where T : struct
    {
        public List<PageState> PageStates = new List<PageState>();
        public List<string> TweenertStateNames = new List<string>(0);
        public bool GoToOutStateOnUnusedPage;
        public string OutStateName;
        public bool AreAllAnimationEqualDuration;
        public float AllAnimationsDuration;

        private Tweenert _tweenert;
        private T _previousSelectedPageInEditor;

        protected override void OnEnable()
        {
            base.OnEnable();

            _tweenert = GetComponent<Tweenert>();
            _previousSelectedPageInEditor = _provider.InEditorVisiblePage;
        }

        private void Start()
        {
            //snap to state of startpage
            foreach (var pagestate in PageStates)
            {
                if (pagestate.OnPage == (int)Enum.ToObject(typeof(T), _provider.CurrentPage))
                {
                    if (!pagestate.DoNothing)
                    {
                        _tweenert.SnapToState(pagestate.StateName);
                    }
                }
            }
        }

        private void Update()
        {
            if (_provider != null && !_previousSelectedPageInEditor.Equals(_provider.InEditorVisiblePage))
            {
                SnapToStepOnPageChangeInEditor();
                _previousSelectedPageInEditor = _provider.InEditorVisiblePage;
            }

            FillTweenertStates();
        }

        private void FillTweenertStates()
        {
            TweenertStateNames = new List<string>(0);

            foreach (var state in _tweenert.States)
            {
                if (!TweenertStateNames.Contains(state.StateName))
                {
                    TweenertStateNames.Add(state.StateName);
                }
            }
        }

        protected override void Provider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AnimateOnPageChange();
        }

        private void SnapToStepOnPageChangeInEditor()
        {
            AnimateOnPageChange(true);
        }

        private void AnimateOnPageChange(bool snap = false)
        {
            foreach (var pagestate in PageStates)
            {
                if (pagestate.OnPage == (int)Enum.ToObject(typeof(T), _provider.CurrentPage))
                {
                    if (!pagestate.DoNothing)
                    {
                        if (snap)
                        {
                            _tweenert.SnapToState(pagestate.StateName);
                        }
                        else
                        {
                            float duration = 0;
                            if (AreAllAnimationEqualDuration)
                            {
                                duration = AllAnimationsDuration;
                            }
                            else
                            {
                                duration = pagestate.Duration;
                            }

                            _tweenert.AnimateToState(pagestate.StateName, duration);
                        }
                    }
                }
            }
        }
    }
}
