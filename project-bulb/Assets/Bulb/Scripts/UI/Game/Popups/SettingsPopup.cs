using Bulb.Core;
using Bulb.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.UI.Game.Popups
{
    public class SettingsPopup : BasePopup
    {
        public static SettingsPopup Instance;
        public static bool AllLevelsUnlocked = false;

        public Button UnlockAllLevelsButton;
        public Button ResetAllLevelsButton;

        public delegate void AllLevelsUnlockedDelegate();
        public event AllLevelsUnlockedDelegate OnAllLevelsUnlocked;

        public delegate void AllLevelsResetDelegate();
        public event AllLevelsResetDelegate OnAllLevelsReset;

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }

        private void OnEnable()
        {
            UnlockAllLevelsButton.onClick.AddListener(UnlockAllLevels);
            ResetAllLevelsButton.onClick.AddListener(ResetAllLevels);
        }

        private void OnDisable()
        {
            UnlockAllLevelsButton.onClick.RemoveListener(UnlockAllLevels);
            ResetAllLevelsButton.onClick.RemoveListener(ResetAllLevels);
        }

        private void UnlockAllLevels()
        {
            AllLevelsUnlocked = true;
            if (OnAllLevelsUnlocked != null)
                OnAllLevelsUnlocked();

            TogglePopup(false);
        }

        private void ResetAllLevels()
        {
            AllLevelsUnlocked = false;
            PlayerPrefs.SetInt(PlayerState.ChapterPlayerPrefsKey, 1);
            PlayerPrefs.SetInt(PlayerState.LevelPlayerPrefsKey, 0);

            if (OnAllLevelsReset != null)
                OnAllLevelsReset();

            TogglePopup(false);
        }
    }
}
