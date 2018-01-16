using Bulb.Core;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Popups
{
    public class SaveLevelPopup : BasePopup
    {
        public RectTransform Content;
        public LevelScrollViewItem LevelItemPrefab;
        public TMP_InputField LevelNameInput;
        public Button SaveButton;

        private ToggleGroup _toggleGroup;

        public delegate void SaveLevelButtonClicked(string levelName);
        public static SaveLevelButtonClicked OnSaveLevelButtonClicked;

        public override void Awake()
        {
            base.Awake();

            if (SaveButton == null)
                Debug.LogWarningFormat("{0} | No save button assigned!", this);

            if (LevelNameInput == null)
                Debug.LogWarningFormat("{0} | No levelname input assigned!", this);

            if (Content == null)
                Debug.LogWarningFormat("{0} | No content object assigned!", this);

            _toggleGroup = Content.GetComponent<ToggleGroup>();

            LevelNameInput.onValueChanged.AddListener((input) =>
            {
                //SaveButton.interactable = !string.IsNullOrEmpty(input);
                _toggleGroup.SetAllTogglesOff();
            });

            SaveButton.onClick.AddListener(() =>
            {
                var levelName = "";

                // If any toggles are on, overwrite that level
                Toggle activeToggle;
                _toggleGroup.ActiveToggles().TryFirstOrDefault(out activeToggle);
                if (activeToggle != null)
                {
                    var levelItem = activeToggle.GetComponent<LevelScrollViewItem>();
                    if (levelItem != null)
                    {
                        levelName = levelItem.LevelName.text;
                    }
                }
                else
                {
                    levelName = LevelNameInput.text;
                }

                if (!string.IsNullOrEmpty(levelName))
                {
                    base.TogglePopup(false);

                    if (OnSaveLevelButtonClicked != null)
                        OnSaveLevelButtonClicked(levelName); 
                }
            });
        }

        public override void TogglePopup(bool show)
        {
            base.TogglePopup(show);

            if(show)
            {
                LevelNameInput.text = "";
                PopulateLevels();
            }
        }

        public void PopulateLevels()
        {
            try
            {
                foreach (Transform child in Content)
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