using System;
using UnityEngine;
using Settings.List;
using System.Reflection;
using Create.UI.Tweenables;
using System.ComponentModel;

namespace Settings.Pagination
{
    public class SettingsManagerVisibility : MonoBehaviour
    {
        public RectTransform Left, Right;
        public int RequiredLeftTaps = 2, RequiredRightTaps = 2;

        private const float TapTimeout = 1;
        private int _leftTapsCount, _rightTapsCount;
        private float _timeOfLastTap;
        private Tweenable _tweenable;

        private bool _isSettingsManagerVisible;
        public bool IsSettingsManagerVisible
        {
            get
            {
                return _isSettingsManagerVisible;
            }
            set
            {
                _isSettingsManagerVisible = value;
                OnPropertyChanged(MethodBase.GetCurrentMethod().Name.Substring(4));
            }
        }

        private void OnEnable()
        {
            _tweenable = GetComponent<Tweenable>();

            SaveSettingsManagerListButton.OnSaveSettingsManagerListButtonClickEvent += SaveSettingsManagerListButton_OnSaveSettingsManagerListButtonClickEvent;

            _tweenable.Snap(false);
        }

        private void OnDisable()
        {
            SaveSettingsManagerListButton.OnSaveSettingsManagerListButtonClickEvent -= SaveSettingsManagerListButton_OnSaveSettingsManagerListButtonClickEvent;
        }

        private void Update()
        {
            /* show or hide on F5 */
            if (Input.GetKeyUp(KeyCode.F5))
            {
                SetVisibility(!_isSettingsManagerVisible);
            }

            /* show or hide on tapsequence */
            // Timeout.
            if (Time.time - _timeOfLastTap > TapTimeout)
            {
                _leftTapsCount = _rightTapsCount = 0;
            }

            // Gesture to open.
            if (Input.GetMouseButtonDown(0))
            {
                // Add a left tap.
                if (RectTransformUtility.RectangleContainsScreenPoint(Left, Input.mousePosition))
                {
                    _leftTapsCount++;
                    _timeOfLastTap = Time.time;
                }
                // If the required number of left taps is reached...
                if (_leftTapsCount == RequiredLeftTaps)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(Right, Input.mousePosition))
                    {
                        _rightTapsCount++;
                        _timeOfLastTap = Time.time;

                        // Open the dev panel if we reached the required amount of left and right taps.
                        if (_rightTapsCount == RequiredRightTaps)
                        {
                            if (SettingsManager.Settings.ShowOnTapSequence.Value)
                            {
                                SetVisibility(!_isSettingsManagerVisible);
                            }
                        }
                    }
                }
            }
        }

        private void SaveSettingsManagerListButton_OnSaveSettingsManagerListButtonClickEvent(object sender, EventArgs e)
        {
            SetVisibility(false);
        }

        public void SetVisibility(bool toState)
        {
            _tweenable.TweenToState(toState);
            IsSettingsManagerVisible = toState;
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