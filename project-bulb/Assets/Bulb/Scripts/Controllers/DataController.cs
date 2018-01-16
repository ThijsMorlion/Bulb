using Bulb.Characters;
using Bulb.Data;
using Bulb.Visuals.Grid;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Bulb.LevelEditor.Popups;

namespace Bulb.Controllers
{
    public class DataController : MonoBehaviour
    {
        private static int _currentLevelDataVersionNumber = 4;

        public delegate void LevelSaved(string levelName);
        public static event LevelSaved OnLevelSaved;

        private ApplicationController _applicationController;
        private BulbGrid _grid;
        private string _extension = ".bulb";

        private void Awake()
        {
            _applicationController = ApplicationController.Instance;
            _grid = FindObjectOfType<BulbGrid>();

            SaveLevelPopup.OnSaveLevelButtonClicked += (input) =>
            {
                SaveLevel(input);
            };

            LoadLevelPopup.OnLoadLevelButtonClicked += (levelName) =>
            {
                LoadLevel(levelName);
            };
        }

        public void SaveLevel(string levelName)
        {
            var saveFileDirectory = Application.dataPath + "/StreamingAssets/EditorLevels/";
            if (Directory.Exists(saveFileDirectory) == false)
                Directory.CreateDirectory(saveFileDirectory);

            var dataPath = saveFileDirectory + levelName + _extension;

            var gridData = _grid.GetSaveData();

            // SAVE CHARACTERS
            var batteries = new List<BatteryCharacterData>();
            var lightBulbs = new List<LightBulbCharacterData>();
            var obstructions = new List<ObstructionCharacterData>();
            var motors = new List<MotorCharacterData>();
            var buzzers = new List<BuzzerCharacterData>();
            var switches = new List<SwitchCharacterData>();
            var characters = _applicationController.CharacterController.Characters;
            characters.ForEach(c =>
            {
                switch (c.Type)
                {
                    case DrawableBase.DrawableType.Battery:
                        batteries.Add(((BatteryCharacter)c).GetSaveData());
                        break;
                    case DrawableBase.DrawableType.Bulb:
                        lightBulbs.Add(((LightBulbCharacter)c).GetSaveData());
                        break;
                    case DrawableBase.DrawableType.Obstruction:
                        obstructions.Add(((ObstructionCharacter)c).GetSaveData());
                        break;
                    case DrawableBase.DrawableType.Motor:
                        motors.Add(((MotorCharacter)c).GetSaveData());
                        break;
                    case DrawableBase.DrawableType.Buzzer:
                        buzzers.Add(((BuzzerCharacter)c).GetSaveData());
                        break;
                    case DrawableBase.DrawableType.Switch:
                        switches.Add(((SwitchCharacter)c).GetSaveData());
                        break;
                }
            });

            // SAVE WIREPIECES
            var wirePieceDataList = new List<WirePieceData>();
            var wirePieces = _applicationController.WireController.WirePieces;
            foreach (var piece in wirePieces)
            {
                wirePieceDataList.Add(piece.GetSaveData());
            }

            // SAVE LEVELDATA
            var currentLevel = _applicationController.LevelController.CurrentLevel;
            var levelData = new LevelData()
            {
                LevelDataVersionNumber = _currentLevelDataVersionNumber,
                GoalDescription = currentLevel.GoalDescription,
                MaxWireAvailable = currentLevel.MaxWire,
                Max4VBatteries = currentLevel.Max4VBatteries,
                Max9VBatteries = currentLevel.Max9VBatteries,
                MaxBulbs = currentLevel.MaxBulbs,
                MaxBuzzers = currentLevel.MaxBuzzers,
                MaxMotors = currentLevel.MaxMotors,
                MaxSwitches = currentLevel.MaxSwitches,
                CanBridge = currentLevel.CanBridge,
                CanSnap = currentLevel.CanSnap,
                CanBranch = currentLevel.CanBranch,
                SuccessByShortCircuit = currentLevel.SuccessByShortCircuit,
                Grid = gridData,
                Batteries = batteries,
                LightBulbs = lightBulbs,
                Obstructions = obstructions,
                Motors = motors,
                Buzzers = buzzers,
                Switches = switches,
                WirePieces = wirePieceDataList
            };

            try
            {
                var jsonData = JsonConvert.SerializeObject(levelData);
                File.WriteAllText(dataPath, jsonData);

                if (OnLevelSaved != null)
                    OnLevelSaved(levelName);

                Debug.LogFormat("{0} | Saved level: {1}", this, levelName);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} | Error while saving level: {1}", this, e.Message);
            }
        }

        public void LoadLevel(string levelName)
        {
            var dataPath = Application.dataPath + "/StreamingAssets/EditorLevels/";
            var levelPath = dataPath + levelName + _extension;

            try
            {
                var data = File.ReadAllText(levelPath);
                var levelData = JsonConvert.DeserializeObject<LevelData>(data);

                CheckForBackwardCompatibility(levelData);

                var levelController = ApplicationController.Instance.LevelController;
                StartCoroutine(levelController.LoadLevel(levelName, levelData));
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} | Error while loading level: {1}", this, e.Message);
            }
        }

        public LevelData GetLevelDataForIngameLevel(int currChapter, int currLevel)
        {
            var saveFileDirectory = Application.streamingAssetsPath + string.Format("/GameLevels/Chapter{0}/", currChapter.ToString("00"));
            var files = Directory.GetFiles(saveFileDirectory, "*.bulb");

            try
            {
                var data = File.ReadAllText(files[currLevel]);
                var levelData = JsonConvert.DeserializeObject<LevelData>(data);

                CheckForBackwardCompatibility(levelData);

                return levelData;
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("{0} | Error while loading level data: {1}", this, e.Message);
                return null;
            }
        }

        private void CheckForBackwardCompatibility(LevelData data)
        {
            if (data.LevelDataVersionNumber < 1)
            {
                data.MaxWireAvailable = 0;
                data.GoalDescription = "";
            }

            if (data.LevelDataVersionNumber < 2)
            {
                data.Max4VBatteries = 0;
                data.Max9VBatteries = 0;
                data.MaxBulbs = 0;
                data.MaxBuzzers = 0;
                data.MaxMotors = 0;
                data.MaxSwitches = 0;
            }

            if (data.LevelDataVersionNumber < 3)
            {
                data.CanBridge = true;
                data.CanSnap = true;
            }

            if (data.LevelDataVersionNumber < 4)
            {
                data.CanBranch = true;
            }

            if (data.LevelDataVersionNumber < 5)
            {
                data.SuccessByShortCircuit = false;
            }
        }
    }
}