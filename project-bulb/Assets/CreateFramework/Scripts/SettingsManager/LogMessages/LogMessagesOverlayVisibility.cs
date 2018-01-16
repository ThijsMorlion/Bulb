using UnityEngine;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace Settings.LogMessages
{
    public class LogMessagesOverlayVisibility : MonoBehaviour
    {
        private IEnumerator _alphaAnimationRoutine;

        private bool _isSettingsOverlayVisible;
        public bool IsSettingsOverlayVisible
        {
            get
            {
                return _isSettingsOverlayVisible;
            }
            set
            {
                _isSettingsOverlayVisible = value;
                OnPropertyChanged(MethodBase.GetCurrentMethod().Name.Substring(4));
            }
        }

        private void AnimateSettingsAlpha(float fromAlpha, float toAlpha, float duration)
        {
            //stop previous animation if it's still animating
            if (_alphaAnimationRoutine != null)
            {
                StopCoroutine(_alphaAnimationRoutine);
            }
            _alphaAnimationRoutine = RunAlphaAnimationRoutine(fromAlpha, toAlpha, duration);
            StartCoroutine(_alphaAnimationRoutine);
        }

        private IEnumerator RunAlphaAnimationRoutine(float fromAlpha, float toAlpha, float duration)
        {
            float lerpPercentage = 0;
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            while (lerpPercentage < 1)
            {
                float lerpAlpha = Settings.Helpers.Mathfx.Hermite(fromAlpha, toAlpha, lerpPercentage);
                canvasGroup.alpha = lerpAlpha;

                lerpPercentage += (Time.deltaTime / duration);
                yield return null;
            }

            if (lerpPercentage >= 1)
            {
                canvasGroup.alpha = toAlpha;
                _alphaAnimationRoutine = null;
            }
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