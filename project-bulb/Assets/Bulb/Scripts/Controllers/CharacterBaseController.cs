using Bulb.Characters;
using Bulb.Data;
using Bulb.Visuals.Grid;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Bulb.Game;

namespace Bulb.Controllers
{
    public class CharacterBaseController : MonoBehaviour
    {
        public List<CharacterBase> CharacterPrefabs;

        public List<CharacterBase> Characters { get; private set; }

        private BulbGrid _grid;

        private void Awake()
        {
            Characters = new List<CharacterBase>();

            _grid = FindObjectOfType<BulbGrid>();
            if (_grid)
                _grid.OnGridChanged += UpdateAllCharacters;
        }

        public void UpdateAllCharacters()
        {
            var debugController = ApplicationController.Instance.DebugController;
            Characters.ForEach(c =>
            {
                c.UpdateCells();

                var gridCell = _grid.GetGridCell((int)c.PositionOnGrid.x, (int)c.PositionOnGrid.y);
                c.transform.position = gridCell.CenterInScreenCoord;
                c.InitializeConnectionPoints();
                c.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(c.GridSize.x * _grid.CellSize, c.GridSize.y * _grid.CellSize);

                c.UpdateConnectionPoints();

                _grid.SetGridCells(gridCell.CenterInScreenCoord, c);
            });
        }

        public void AddCharacter(CharacterBase character, Vector2 pos)
        {
            var canvasController = ApplicationController.Instance.CanvasController;
            var charTransform = character.GetComponent<RectTransform>();
            charTransform.SetParent(canvasController.CharacterContainer, false);
            charTransform.position = pos;

            Characters.Add(character);
            character.InitializeConnectionPoints();
            character.UpdateConnectionPoints();
        }

        public void DeleteCharacter(CharacterBase character)
        {
            character.RemoveAllConnections();
            character.ClearCells();
            DestroyImmediate(character.gameObject);
            Characters.Remove(character);
        }

        public void ClearAllCharacters()
        {
            foreach (var character in Characters)
            {
                character.ClearCells();
                DestroyImmediate(character.gameObject);
            }

            Characters.Clear();
        }

        public void ResetAllCharacters()
        {
            foreach (var character in Characters)
            {
                character.Reset();
            }
        }

        public void ResetAllButExcludedCharacters(List<CharacterBase> excludedChars)
        {
            foreach (var character in Characters)
            {
                if (!excludedChars.Contains(character))
                    character.Reset();
            }
        }

        public void LoadCharacters<T1, T2>(List<T2> data) where T1 : CharacterBase
                                                          where T2 : CharacterData
        {
            if (data != null)
            {
                data.ForEach(d =>
                {
                    var gridPos = new Vector2(d.PositionOnGrid.x, d.PositionOnGrid.y);
                    var gridCell = _grid.GetGridCell((int)gridPos.x, (int)gridPos.y);

                    if (gridCell != null)
                    {
                        var charInstance = InstantiateCharacter<T1>(d.Type, d, gridCell);

                        switch (d.Type)
                        {
                            case DrawableBase.DrawableType.Battery:
                                var batteryData = Convert.ChangeType(d, typeof(BatteryCharacterData)) as BatteryCharacterData;
                                var batteryInstance = Convert.ChangeType(charInstance, typeof(BatteryCharacter)) as BatteryCharacter;
                                batteryInstance.Battery = batteryData.Params;
                                break;
                            case DrawableBase.DrawableType.Bulb:
                                var bulbData = Convert.ChangeType(d, typeof(LightBulbCharacterData)) as LightBulbCharacterData;
                                var bulbInstance = Convert.ChangeType(charInstance, typeof(LightBulbCharacter)) as LightBulbCharacter;
                                bulbInstance.Light = bulbData.Params;
                                break;
                            case DrawableBase.DrawableType.Motor:
                                var motorData = Convert.ChangeType(d, typeof(MotorCharacterData)) as MotorCharacterData;
                                var motorInstance = Convert.ChangeType(charInstance, typeof(MotorCharacter)) as MotorCharacter;
                                motorInstance.MotorParams = motorData.Params;
                                break;
                            case DrawableBase.DrawableType.Buzzer:
                                var buzzerData = Convert.ChangeType(d, typeof(BuzzerCharacterData)) as BuzzerCharacterData;
                                var buzzerInstance = Convert.ChangeType(charInstance, typeof(BuzzerCharacter)) as BuzzerCharacter;
                                buzzerInstance.BuzzerParams = buzzerData.Params;
                                break;
                        }

                        AddCharacter(charInstance, gridCell.CenterInScreenCoord);

                        if (GameState.CurrentState == GameStates.Game)
                        {
                            charInstance.CanBeSelected = false;
                            charInstance.CanBeMoved = false;
                            charInstance.CanBeRotated = false;
                            charInstance.Cells.ForEach(c => c.SetNonEditable(true));
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("{0} | No gridcell position found at {1} for character {2}", this, gridPos, d.Type);
                    }
                });

                UpdateAllCharacters();
            }
        }

        private T InstantiateCharacter<T>(DrawableBase.DrawableType type, CharacterData data, GridCell cell) where T : CharacterBase
        {
            CharacterBase prefab = null;
            if (type == DrawableBase.DrawableType.Battery)
            {
                var batteryBaseChars = CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Battery);
                var batteryData = Convert.ChangeType(data, typeof(BatteryCharacterData)) as BatteryCharacterData;
                foreach (var battery in batteryBaseChars)
                {
                    if (((BatteryCharacter)battery).Battery.Voltage == batteryData.Params.Voltage)
                    {
                        prefab = battery;
                    }
                }
            }
            else
            {
                var baseChar = CharacterPrefabs.Where(c => c.Type == type).SingleOrDefault();
                if (baseChar != null)
                    prefab = baseChar;
            }

            if (prefab != null)
            {
                var charInstance = Instantiate(prefab.gameObject).GetComponent<CharacterBase>();
                charInstance.transform.SetParent(ApplicationController.Instance.CanvasController.CharacterContainer, false);
                charInstance.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(prefab.GridSize.x * _grid.CellSize, prefab.GridSize.y * _grid.CellSize);
                charInstance.PositionOnGrid = new Vector2(data.PositionOnGrid.x, data.PositionOnGrid.y);
                charInstance.SetRotationAroundPivot(data.Rotation);
                _grid.SetGridCells(cell.CenterInScreenCoord, charInstance);

                return (T)charInstance;
            }

            return null;
        }
    }
}