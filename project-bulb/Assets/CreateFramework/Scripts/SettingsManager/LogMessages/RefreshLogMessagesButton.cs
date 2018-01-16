using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Settings.LogMessages
{
    public class RefreshLogMessagesButton : MonoBehaviour
    {
        public static event EventHandler RefreshLogsButtonClickEvent;
        private IEnumerator _rotationAnimationRoutine;

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(() => OnButtonClick());
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }

        private void OnButtonClick()
        {
            if (RefreshLogsButtonClickEvent != null)
            {
                RefreshLogsButtonClickEvent(this, null);
            }

            RotateArrow();
        }

        private void RotateArrow()
        {
            AnimateSettingsAlpha(360, 0, 0.5f);
        }

        private void AnimateSettingsAlpha(float fromValue, float toValue, float duration)
        {
            //stop previous animation if it's still animating
            if (_rotationAnimationRoutine != null)
            {
                StopCoroutine(_rotationAnimationRoutine);
            }
            _rotationAnimationRoutine = RunRotateRoutine(fromValue, toValue, duration);
            StartCoroutine(_rotationAnimationRoutine);
        }


        private IEnumerator RunRotateRoutine(float fromValue, float toValue, float duration)
        {
            float lerpPercentage = 0;

            while (lerpPercentage < 1)
            {
                float lerpedValue = Settings.Helpers.Mathfx.Hermite(fromValue, toValue, lerpPercentage);
                transform.localEulerAngles = new Vector3(0, 0, lerpedValue);

                lerpPercentage += (Time.deltaTime / duration);
                yield return null;
            }

            if (lerpPercentage >= 1)
            {
                transform.localEulerAngles = new Vector3(0, 0, toValue);
                _rotationAnimationRoutine = null;
            }
        }
    }
}