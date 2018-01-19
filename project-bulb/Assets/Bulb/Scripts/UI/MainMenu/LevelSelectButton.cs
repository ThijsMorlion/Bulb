using Bulb.Controllers;
using Bulb.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.UI.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class LevelSelectButton : MonoBehaviour
    {
        private int _levelIndex = int.MinValue;
        public int LevelIndex
        {
            get
            {
                return _levelIndex;
            }

            set
            {
                _levelIndex = value;
                var textElement = GetComponentInChildren<TextMeshProUGUI>();
                textElement.text = _levelIndex.ToString();
            }
        }

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(OnButtonClicked);

            LevelIndex = int.Parse(name);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnButtonClicked);
        }

        private void Start()
        {
            if (Debug.isDebugBuild == false)
            {
                var currChapter = ApplicationController.Instance.ChapterController.CurrentChapterIndex;
                GetComponent<Button>().interactable = PlayerPrefs.GetInt(PlayerState.ChapterPlayerPrefsKey, 0) > currChapter ||
                                                      PlayerPrefs.GetInt(PlayerState.ChapterPlayerPrefsKey, 0) == currChapter && PlayerPrefs.GetInt(PlayerState.LevelPlayerPrefsKey, 0) >= LevelIndex - 1;
            }
        }

        private void OnButtonClicked()
        {
            var levelController = ApplicationController.Instance.LevelController;
            levelController.LoadLevelInGame(LevelIndex - 1);
        }
    }
}
