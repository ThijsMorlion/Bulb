using UnityEngine;
using Settings.Model;

namespace Settings.PerformanceStats
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PerformanceStatsPanelVisibility : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        private void OnEnable()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            SettingsManager.PropertyChanged += UnitySettingsManager_PropertyChanged;
        }

        private void OnDisable()
        {
            SettingsManager.PropertyChanged += UnitySettingsManager_PropertyChanged;
        }

        private void UnitySettingsManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Settings")
            {
                if (SettingsManager.Settings != null)
                {
                    SettingsManager.Settings.ShowStatistics.PropertyChanged += ShowStatistics_PropertyChanged;
                    SetVisibility(SettingsManager.Settings.ShowStatistics.Value);
                }
            }
        }

        private void ShowStatistics_PropertyChanged(object sender, SettingChangedEventArgs<bool> e)
        {
            SetVisibility(e.NewValue);
        }

        private void SetVisibility(bool makeVisible)
        {
            if (_canvasGroup == null)
            {
                Debug.Log("Could not set visibility of performance stats because it has no CanvasGroup component");
                return;
            }

            if (makeVisible)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.blocksRaycasts = true;
            }
            else
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}