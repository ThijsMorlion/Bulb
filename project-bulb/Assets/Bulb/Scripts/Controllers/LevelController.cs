using System;
using System.Collections;
using System.Collections.Generic;
using Bulb.Characters;
using Bulb.Core;
using Bulb.Data;
using Bulb.Game;
using Bulb.UI.Game.Popups;
using Bulb.Visuals.Grid;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

namespace Bulb.Controllers
{
    public class LevelController : MonoBehaviour
    {
        public delegate void LevelLoaded(string levelName, LevelData data);
        public static event LevelLoaded OnLevelLoaded;

        public int CurrentLevelIndex { get; private set; }
        public Level CurrentLevel { get; private set; }

        private BulbGrid _grid;

        private void Start()
        {
            CreateNewLevel();
            CurrentLevelIndex = -1;
        }

        private void OnEnable()
        {
            SelectionController.OnObjectDeleted += SelectionController_OnObjectDeleted;
        }

        private void OnDisable()
        {
            SelectionController.OnObjectDeleted -= SelectionController_OnObjectDeleted;
        }

        private void SelectionController_OnObjectDeleted(DrawableBase drawable)
        {
            switch (drawable.Type)
            {
                case DrawableBase.DrawableType.Battery:
                    var battery = (BatteryCharacter)drawable;
                    if (battery.Battery.Voltage == 4)
                        ++CurrentLevel.Max4VBatteries;
                    else
                        ++CurrentLevel.Max9VBatteries;
                    break;
                case DrawableBase.DrawableType.Bulb:
                    ++CurrentLevel.MaxBulbs;
                    break;
                case DrawableBase.DrawableType.Buzzer:
                    ++CurrentLevel.MaxBuzzers;
                    break;
                case DrawableBase.DrawableType.Motor:
                    ++CurrentLevel.MaxMotors;
                    break;
                case DrawableBase.DrawableType.Switch:
                    ++CurrentLevel.MaxSwitches;
                    break;
            }
        }

        public void CreateNewLevel()
        {
            if (CurrentLevel != null)
            {
                CurrentLevel.Clean();
                CurrentLevel = null;
            }

            CurrentLevel = new Level();
        }

        public void LoadNextLevel()
        {
            LoadLevelInGame(CurrentLevelIndex + 1);
        }

        public void LoadLevelInGame(int index)
        {
            var chapterController = ApplicationController.Instance.ChapterController;
            if (index < 0 || index >= chapterController.CurrentNumberOfLevels)
            {
                Debug.LogWarning("Given level index exceeds number of loaded levels!");
                GoToChapterMain();
                return;
            }

            CurrentLevelIndex = index;
            StartCoroutine(LoadLevelAsync());
        }

        public void GoToChapterMain()
        {
            Debug.LogFormat("Go to Main Menu of Chapter{0}", ApplicationController.Instance.ChapterController.CurrentChapterIndex.ToString("00"));
            SceneManager.LoadScene(string.Format("Chapter{0}", ApplicationController.Instance.ChapterController.CurrentChapterIndex.ToString("00")));
        }

        private IEnumerator LoadLevelAsync()
        {
            var op = SceneManager.LoadSceneAsync("GAME", LoadSceneMode.Single);
            op.completed += LoadLevelAsyncCompleted;
            yield return null;
        }

        private void LoadLevelAsyncCompleted(AsyncOperation obj)
        {
            _grid = FindObjectOfType<BulbGrid>();
            if (_grid)
            {
                var currentChapterIndex = ApplicationController.Instance.ChapterController.CurrentChapterIndex;

                try
                {
                    var levelData = ApplicationController.Instance.DataController.GetLevelDataForIngameLevel(currentChapterIndex, CurrentLevelIndex);

                    if (GameState.CurrentState == GameStates.Game)
                    {
                        BulbEvents.StartTimer();

                        var analytics = ApplicationController.Instance.AnalyticsController;
                        var chapterIndexParam = new KeyValuePair<string, object>(BulbEvents.ChapterIndexParam, currentChapterIndex);
                        var levelIndexParam = new KeyValuePair<string, object>(BulbEvents.LevelIndexParam, CurrentLevelIndex);
                        analytics.LogCustomEvent(BulbEvents.Category_UserEvents, BulbEvents.Action_Level, BulbEvents.LevelStarted, chapterIndexParam, levelIndexParam);
                    }

                    StartCoroutine(LoadLevel(CurrentLevelIndex.ToString(), levelData));
                }
                catch (Exception e)
                {
                    Debug.LogWarningFormat("{0} | Error while loading level: {1}", this, e.Message);
                }
            }
            else
            {
                Debug.LogError("No grid found in game scene!");
            }
        }

        public IEnumerator LoadLevel(string levelName, LevelData levelData)
        {
            if (CurrentLevel != null)
            {
                CurrentLevel.Clean();
                CurrentLevel = null;
            }

            CurrentLevel = new Level()
            {
                GoalDescription = levelData.GoalDescription,
                Max4VBatteries = levelData.Max4VBatteries,
                Max9VBatteries = levelData.Max9VBatteries,
                MaxBulbs = levelData.MaxBulbs,
                MaxBuzzers = levelData.MaxBuzzers,
                MaxMotors = levelData.MaxMotors,
                MaxSwitches = levelData.MaxSwitches,
                MaxWire = levelData.MaxWireAvailable,
                CanBridge = levelData.CanBridge,
                CanSnap = levelData.CanSnap,
                CanBranch = levelData.CanBranch,
                SuccessByShortCircuit = levelData.SuccessByShortCircuit
            };

            var characterController = ApplicationController.Instance.CharacterController;
            characterController.ClearAllCharacters();

            var wireController = ApplicationController.Instance.WireController;
            wireController.ClearAllWirePieces();

            _grid = FindObjectOfType<BulbGrid>();
            _grid.LoadSaveData(levelData.Grid);

            yield return null;

            characterController.LoadCharacters<BatteryCharacter, BatteryCharacterData>(levelData.Batteries);
            characterController.LoadCharacters<LightBulbCharacter, LightBulbCharacterData>(levelData.LightBulbs);
            characterController.LoadCharacters<ObstructionCharacter, ObstructionCharacterData>(levelData.Obstructions);
            characterController.LoadCharacters<MotorCharacter, MotorCharacterData>(levelData.Motors);
            characterController.LoadCharacters<BuzzerCharacter, BuzzerCharacterData>(levelData.Buzzers);
            characterController.LoadCharacters<SwitchCharacter, SwitchCharacterData>(levelData.Switches);

            characterController.UpdateAllCharacters();

            wireController.LoadSaveData(levelData.WirePieces);

            _grid.FillZoomGrid();

            if (GameState.CurrentState == GameStates.Game)
            {
                var instructionPopup = InstructionsPopup.Instance;
                instructionPopup.SetDescription(levelData.GoalDescription);
                instructionPopup.TogglePopup(true);
            }

            if (OnLevelLoaded != null)
                OnLevelLoaded(levelName, levelData);

            _grid.SetGridMode(BulbGrid.GridMode.DrawWire);

            Debug.LogFormat("{0} | Loaded level: {1}", this, levelName);
        }

        public static string GetFileProtocol()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return "file:///";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return "file://";
            }
            return "";
        }
    }
}
