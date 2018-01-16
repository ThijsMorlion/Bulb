using Bulb.Controllers;
using Bulb.Game;
using UnityEngine;

namespace Bulb.UI.MainMenu
{
    [RequireComponent(typeof(LineRenderer))]
    public class ChapterViewWire: MonoBehaviour
    {
        public Material ActiveMaterial;
        public Material DefaultMaterial;

        public float ActiveWidth = .1f;
        public float DefaultWidth = .03f;

        public int LevelIndex;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.material = DefaultMaterial;
        }

        private void Start()
        {
            var currChapter = ApplicationController.Instance.ChapterController.CurrentChapterIndex;
            var isActive = PlayerPrefs.GetInt(PlayerState.ChapterPlayerPrefsKey, 0) > currChapter ||
                                 PlayerPrefs.GetInt(PlayerState.ChapterPlayerPrefsKey, 0) == currChapter && PlayerPrefs.GetInt(PlayerState.LevelPlayerPrefsKey, 0) >= LevelIndex - 1;

            _lineRenderer.material = isActive ? ActiveMaterial : DefaultMaterial;
            _lineRenderer.widthMultiplier = isActive ? ActiveWidth : DefaultWidth;
        }
    }
}
