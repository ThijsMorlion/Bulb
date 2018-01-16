using Bulb.Controllers;
using Bulb.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace Bulb.UI.Game.Popups
{
    public class InstructionsPopup : BasePopup
    {
        public static InstructionsPopup Instance;

        public TextMeshProUGUI Text;

        private VideoPlayer _videoPlayer;

        public override void Awake()
        {
            base.Awake();

            _videoPlayer = GetComponentInChildren<VideoPlayer>();
            Instance = this;
        }

        public void SetDescription(string message)
        {
            Text.text = message;
        }

        public override void TogglePopup(bool show)
        {
            if (show)
            {
                var videoLoading = LoadVideo();
                if (!videoLoading && !string.IsNullOrEmpty(Text.text))
                    base.TogglePopup(true);
            }
            else
            {
                _videoPlayer.Stop();
                base.TogglePopup(false);
            }
        }

        private bool LoadVideo()
        {
            var currChapter = ApplicationController.Instance.ChapterController.CurrentChapterIndex;
            var currLevel = ApplicationController.Instance.LevelController.CurrentLevelIndex;

            try
            {
                var videoClip = Resources.Load<VideoClip>(string.Format("InstructionVideos/{0}/{1}", currChapter.ToString("00"), currLevel.ToString("00")));

                if (videoClip != null)
                {
                    _videoPlayer.gameObject.SetActive(true);
                    _videoPlayer.clip = videoClip;
                    _videoPlayer.Play();
                    _videoPlayer.prepareCompleted += _videoPlayer_prepareCompleted;
                    return true;
                }
                else
                {
                    _videoPlayer.gameObject.SetActive(false);
                    _videoPlayer.clip = null;
                    _videoPlayer.Stop();
                    return false;
                }
            }
            catch (System.Exception)
            {
                Debug.LogErrorFormat("Failed to load video for Chapter {0}, Level {1}!", currChapter, currLevel);
                return false;
            }
        }

        private void _videoPlayer_prepareCompleted(VideoPlayer source)
        {
            base.TogglePopup(true);
        }
    }
}