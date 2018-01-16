using Bulb.Controllers;
using UnityEngine;

namespace Bulb.UI.MainMenu
{
    public class LevelButtonSpawner : MonoBehaviour
    {
        public LevelSelectButton LevelButtonPrefab;

        private void Start()
        {
            InstantiateLevelButtons();
        }

        private void InstantiateLevelButtons()
        {
            var yPosDiff = 150;
            var currNumberOfLevels = ApplicationController.Instance.ChapterController.CurrentNumberOfLevels;

            for (var i = 0; i < currNumberOfLevels; ++i)
            {
                var levelButton = Instantiate(LevelButtonPrefab, transform, false);
                levelButton.LevelIndex = i + 1;

                var levelRectT = levelButton.GetComponent<RectTransform>();
                levelRectT.anchoredPosition = new Vector2(0, (i) * -yPosDiff);
            }

            var rectT = GetComponent<RectTransform>();
            rectT.sizeDelta = new Vector2(rectT.sizeDelta.x, currNumberOfLevels * yPosDiff);
        }
    }
}
