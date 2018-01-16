using Bulb.Controllers;
using UnityEngine;

namespace Bulb.Game
{
    public class PlayerState
    {
        public const string ChapterPlayerPrefsKey = "Chapter";
        public const string LevelPlayerPrefsKey = "Level";

        public static void UnlockNextLevel()
        {
            var levelController = ApplicationController.Instance.LevelController;
            var chapterController = ApplicationController.Instance.ChapterController;
            if (chapterController.CurrentChapterIndex < PlayerPrefs.GetInt(ChapterPlayerPrefsKey, 0) ||
                chapterController.CurrentChapterIndex == PlayerPrefs.GetInt(ChapterPlayerPrefsKey, 0) && levelController.CurrentLevelIndex < PlayerPrefs.GetInt(LevelPlayerPrefsKey, 0))
            {
                return;
            }

            // If last level reached end vertical slice
            if (levelController.CurrentLevelIndex + 1 > chapterController.CurrentNumberOfLevels)
            {
                return;
            }
            else
            {
                PlayerPrefs.SetInt(LevelPlayerPrefsKey, levelController.CurrentLevelIndex + 1);
            }
        }
    }
}
