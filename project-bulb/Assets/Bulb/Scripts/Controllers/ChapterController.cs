using System.IO;
using Bulb.Game;
using Bulb.UI.MainMenu;
using UnityEngine;

namespace Bulb.Controllers
{
    public class ChapterController : MonoBehaviour
    {
        public int CurrentChapterIndex { get; set; }
        public int NumberOfChapters { get; set; }

        private int _currentNumberOfLevels;
        public int CurrentNumberOfLevels
        {
            get
            {
                if(_currentNumberOfLevels == 0)
                {
                    var saveFileDirectory = Application.streamingAssetsPath + string.Format("/GameLevels/Chapter{0}/", CurrentChapterIndex.ToString("00"));
                    var files = Directory.GetFiles(saveFileDirectory, "*.bulb");

                    _currentNumberOfLevels = files.Length;
                }

                return _currentNumberOfLevels;
            }
        }

        private void Awake()
        {
            CurrentChapterIndex = 1;

            // Change this to dynamic number
            NumberOfChapters = 1;

            PlayerPrefs.SetInt(PlayerState.ChapterPlayerPrefsKey, 1);
        }
    }
}