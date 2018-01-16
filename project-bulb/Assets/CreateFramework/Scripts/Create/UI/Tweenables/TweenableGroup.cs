using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using UnityEngine.Events;

namespace Create.UI.Tweenables
{
    [ExecuteInEditMode]
    public class TweenableGroup : MonoBehaviour
    {
        [Tooltip("Are the delays of the tweenables ordered from the out state to the in state?")]
        public bool DelaysAreFromOutToIn = true;

        private Tweenable[] _tweenables;
        private IEnumerator[] _coroutines;
        private int _childTweenablesCompleted = 0;

        public TweenableEvent OnStarted, OnCompleted;

        void OnEnable()
        {
            _tweenables = GetComponentsInChildren<Tweenable>();
            _coroutines = new IEnumerator[_tweenables.Length];
        }

        public void TweenToState(bool toIn)
        {
            if (!Application.isPlaying)
            {
                Snap(toIn);
                return;
            }

            StopCoroutines();

            _tweenables = GetComponentsInChildren<Tweenable>();
            _coroutines = new IEnumerator[_tweenables.Length];
            if (_tweenables == null || _tweenables.Length == 0)
                return;

            float maxDelay = _tweenables.Max(t => t.Delay);
            for (int i = 0; i < _tweenables.Length; i++)
            {
                var tweenable = _tweenables[i];

                if (tweenable.IgnoreInTweenableGroup)
                    continue;

                float delay = tweenable.Delay;

                // 1-x the delays if the transition direction is opposite to the delay direction.
                if (!tweenable.NoDelayMirrorInTweenableGroup && !toIn && DelaysAreFromOutToIn || toIn && !DelaysAreFromOutToIn)
                {
                    delay = maxDelay - delay;
                }

                _coroutines[i] = RunTweenWithDelay(tweenable, toIn, delay);
                StartCoroutine(_coroutines[i]);

                // Fire and forget completed event on child tweenable
                UnityAction<bool> completedAction = null;
                completedAction = (value) =>
                {
                    ++_childTweenablesCompleted;

                    if (_childTweenablesCompleted == _coroutines.Length)
                    {
                        if (OnCompleted != null)
                            OnCompleted.Invoke(toIn);

                        _childTweenablesCompleted = 0;
                    }

                    tweenable.OnCompleted.RemoveListener(completedAction);
                };

                tweenable.OnCompleted.AddListener(completedAction);
            }

            if (OnStarted != null)
                OnStarted.Invoke(toIn);
        }

        private IEnumerator RunTweenWithDelay(Tweenable tweenable, bool toIn, float delay)
        {
            if(tweenable.DelayMode == DelayModes.Both || tweenable.DelayMode == DelayModes.InOnly && toIn || tweenable.DelayMode == DelayModes.OutOnly && !toIn)
                yield return new WaitForSecondsRealtime(delay);

            tweenable.TweenToState(toIn);
        }

        public void Snap(bool toIn)
        {
            StopCoroutines();

            _tweenables = GetComponentsInChildren<Tweenable>();
            foreach (var tweenable in _tweenables)
            {
                if (!tweenable.IgnoreInTweenableGroup)
                {
                    tweenable.Snap(toIn);
                }
            }
        }

        private void StopCoroutines()
        {
            if (_coroutines == null)
                return;

            // Stop all running coroutines.
            for (int i = 0; i < _coroutines.Length; i++)
            {
                if (_coroutines[i] != null)
                {
                    StopCoroutine(_coroutines[i]);
                }
            }
        }
    }

    [Serializable]
    public class TweenableGroupEvent : UnityEvent<bool> { }
}