using System;
using UnityEngine;
using System.ComponentModel;
using System.Collections.Generic;

namespace Create.UI.Tweenerts
{
    [ExecuteInEditMode]
    public class TweenertGroup : MonoBehaviour
    {
        [NonSerialized]
        public float Duration;
        public List<string> StatesInChildren = new List<string>(0);

        private string _state;
        public string State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
                OnPropertyChanged(GetPropertyName(() => State));
            }
        }

        private void Update()
        {
            FillStatesInChildrenList();
        }

        private void FillStatesInChildrenList()
        {
            StatesInChildren = new List<string>(0);

            if (GetComponent<Tweenert>() != null)
            {
                foreach (var state in GetComponent<Tweenert>().States)
                {
                    if (!string.IsNullOrEmpty(state.StateName))
                    {
                        StatesInChildren.Add(state.StateName);
                    }
                }
            }

            foreach (var tweenert in GetComponentsInChildren<Tweenert>())
            {
                foreach (var state in tweenert.States)
                {
                    if (!string.IsNullOrEmpty(state.StateName) && !tweenert.IgnoreInGroup) //don't add to list when name is empty or tweenert is ignored in group anyway
                    {
                        if (!StatesInChildren.Contains(state.StateName))
                        {
                            StatesInChildren.Add(state.StateName);
                        }
                    }
                }
            }
        }

        public void AnimateToState(string stateName, float duration)
        {
            Duration = duration;
            State = stateName;
        }

        public void SnapToState(string stateName)
        {
            AnimateToState(stateName, 0);
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
