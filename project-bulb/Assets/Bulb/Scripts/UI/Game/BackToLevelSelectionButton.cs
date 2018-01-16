using System.Collections.Generic;
using Bulb.Controllers;
using Bulb.Core;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace Bulb.UI.Game
{
    [RequireComponent(typeof(Button))]
    public class BackToLevelSelectionButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                var levelController = ApplicationController.Instance.LevelController;

                var analytics = ApplicationController.Instance.AnalyticsController;
                var chapterIndexParam = new KeyValuePair<string, object>(BulbEvents.ChapterIndexParam, ApplicationController.Instance.ChapterController.CurrentChapterIndex);
                var levelIndexParam = new KeyValuePair<string, object>(BulbEvents.LevelIndexParam, levelController.CurrentLevelIndex);
                var levelCompletionTime = new KeyValuePair<string, object>(BulbEvents.LevelPlayTime, BulbEvents.FinishTimer().ToString());
                var result = analytics.LogCustomEvent(BulbEvents.Category_UserEvents, BulbEvents.Action_Level, BulbEvents.LevelQuit, chapterIndexParam, levelIndexParam, levelCompletionTime);

                if (result)
                    levelController.GoToChapterMain();
            });
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
}
