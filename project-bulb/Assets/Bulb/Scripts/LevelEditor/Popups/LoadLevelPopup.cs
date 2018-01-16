using Bulb.Core;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Popups
{
    public class LoadLevelPopup : BasePopup
    {
        public Button LoadButton;
        public RectTransform Content;
        public LevelScrollViewItem LevelItemPrefab;

        public delegate void LoadLevelButtonClicked(string levelName);
        public static LoadLevelButtonClicked OnLoadLevelButtonClicked;

        private ToggleGroup _toggleGroup;

        public override void Awake()
        {
            base.Awake();

            if (LoadButton == null)
                Debug.LogWarningFormat("{0} | No load button assigned!", this);

            if (Content == null)
                Debug.LogWarningFormat("{0} | No content object assigned!", this);

            _toggleGroup = Content.GetComponent<ToggleGroup>();

            if (LevelItemPrefab == null)
                Debug.LogWarningFormat("{0} | No level item prefab assigned!", this);

            LoadButton.onClick.AddListener(() =>
            {
                base.TogglePopup(false);

                Toggle activeToggle;
                _toggleGroup.ActiveToggles().TryFirstOrDefault(out activeToggle);
                if (activeToggle != null)
                {
                    var levelItem = activeToggle.GetComponent<LevelScrollViewItem>();
                    if (levelItem != null)
                    {
                        if (OnLoadLevelButtonClicked != null)
                            OnLoadLevelButtonClicked(levelItem.LevelName.text);
                    }
                }
            });
        }

        public override void TogglePopup(bool show)
        {
            base.TogglePopup(show);

            PopulateLevels();
        }

        public void PopulateLevels()
        {
            try
            {
                foreach(Transform child in Content)
                {
                    Destroy(child.gameObject);
                }

                var saveFileDirectory = Application.dataPath + "/StreamingAssets/EditorLevels/";
                var files = Directory.GetFiles(saveFileDirectory);

                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file);
                    if (extension.Equals(".bulb"))
                    {
                        var levelItem = Instantiate(LevelItemPrefab, Content, false);
                        levelItem.Toggle.group = _toggleGroup;
                        levelItem.LevelName.text = Path.GetFileNameWithoutExtension(file);
                    }
                }

                _toggleGroup.SetAllTogglesOff();
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} | Error while loading level names: {1}", this, e.Message);
            }
        }
    }
}