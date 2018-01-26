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
            var childButton = LevelButtonsContainer.GetChild((LevelButtonsContainer.childCount - currLevel) - 1);
            var childPos = childButton.GetComponent<RectTransform>().anchoredPosition.y;

            var chapterViewPos = Mathf.Abs(childPos) - GetComponentInParent<RectTransform>().rect.height / 2;
            StartCoroutine(GetComponentInParent<TouchScroll>().SetAnchoredPosition(new Vector2(0, chapterViewPos)));
        }
    }
}
