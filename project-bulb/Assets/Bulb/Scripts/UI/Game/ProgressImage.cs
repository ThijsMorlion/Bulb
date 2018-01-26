using Bulb.Controllers;
using Bulb.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Bulb.Scripts.UI.Game
{
    public class ProgressImage : MonoBehaviour
    {
        public int LevelIndex;

        [Range(0, 1)]
        public float AlphaMultiplier;
        private float _previousAlphaMultiplier = 0f;

        private void Awake()
        {
            GetComponent<Image>().material.SetFloat("_AlphaMultiplier", 0f);
            ApplicationController.Instance.LevelController.RegisterWorldViewAnimation(LevelIndex);
        }

        private void Start()
        {
            var currChapter = ApplicationController.Instance.ChapterController.CurrentChapterIndex;
            var playerLevel = PlayerPrefs.GetInt(PlayerState.LevelPlayerPrefsKey, 0);
            var isActive = PlayerPrefs.GetInt(PlayerState.ChapterPlayerPrefsKey, 0) > currChapter ||
                           PlayerPrefs.GetInt(PlayerState.ChapterPlayerPrefsKey, 0) == currChapter && PlayerPrefs.GetInt(PlayerState.LevelPlayerPrefsKey, 0) >= LevelIndex - 1;

            gameObject.SetActive(isActive);

            var animator = GetComponent<Animator>();
            if (animator && isActive)
            {
                if (playerLevel != LevelIndex)
                    animator.SetBool("instantSwitchingOn", true);
                else
                    animator.SetBool("switchingOn", true);
            }

        }

        private void Update()
        {
            if (_previousAlphaMultiplier != AlphaMultiplier)
            {
                GetComponent<Image>().material.SetFloat("_AlphaMultiplier", AlphaMultiplier);
                _previousAlphaMultiplier = AlphaMultiplier;
            }
        }
    }
}
