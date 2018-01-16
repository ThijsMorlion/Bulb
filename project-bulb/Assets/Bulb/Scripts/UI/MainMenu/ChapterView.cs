using Bulb.Game;
using UnityEngine;

namespace Bulb.UI.MainMenu
{
    public class ChapterView : MonoBehaviour
    {
        public RectTransform LevelButtonsContainer;

        public void Start()
        {
            var currLevel = PlayerPrefs.GetInt(PlayerState.LevelPlayerPrefsKey, 0);
            var childButton = LevelButtonsContainer.GetChild((LevelButtonsContainer.childCount - 1) - currLevel);
            var childPos = childButton.GetComponent<RectTransform>().anchoredPosition.y;

            var chapterViewPos = Mathf.Abs(childPos) - GetComponentInParent<RectTransform>().rect.height / 2;
            GetComponentInParent<TouchScroll>().SetAnchoredPosition2D(new Vector2(0, chapterViewPos));
        }
    }
}
