using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Settings.List.Description
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SettingDescriptionVisibility : MonoBehaviour
    {
        public GameObject CloseDescriptionButton;

        private IEnumerator _alphaAnimationRoutine;
        private SettingDescription _settingDescription;

        private void OnEnable()
        {
            CloseDescriptionButton.GetComponent<Button>().onClick.AddListener(() => OnCloseButtonClick());

            _settingDescription = FindObjectOfType<SettingDescription>();
            if (_settingDescription != null)
            {
                _settingDescription.PropertyChanged += SettingDescription_PropertyChanged;
            }

            HideOverlay();
        }

        private void OnDisable()
        {
            CloseDescriptionButton.GetComponent<Button>().onClick.RemoveAllListeners();

            if (_settingDescription != null)
            {
                _settingDescription.PropertyChanged += SettingDescription_PropertyChanged;
            }
        }

        private void SettingDescription_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowOverlay();
        }

        private void OnCloseButtonClick()
        {
            HideOverlay();
        }

        private void ShowOverlay()
        {
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            AnimateSettingsAlpha(GetComponent<CanvasGroup>().alpha, 1f, 0.5f);
        }

        private void HideOverlay()
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            AnimateSettingsAlpha(GetComponent<CanvasGroup>().alpha, 0f, 0.5f);
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
    }
}