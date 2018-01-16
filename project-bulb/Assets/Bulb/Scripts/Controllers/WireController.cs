using Bulb.Characters;
using Bulb.Characters.Wire;
using Bulb.Core;
using Bulb.Data;
using Bulb.Visuals.Grid;
using System.Collections.Generic;
using UnityEngine;
using System;
using Bulb.Game;
using Settings.Model;

namespace Bulb.Controllers
{
    public class WireController : MonoBehaviour
    {
        public Transform WirePiecePrefab;

        private BulbGrid _grid;
        private CanvasController _canvasController;

        private HashSet<WirePiece> _wirePieces = new HashSet<WirePiece>();
        public HashSet<WirePiece> WirePieces { get { return _wirePieces; } }

        public WirePiece PreviousSelectedWirePiece { get; set; }

        private void Awake()
        {
            _grid = FindObjectOfType<BulbGrid>();
            if (_grid)
            {
                _grid.OnGridChanged += UpdateVisuals;
                _grid.OnGridChanged += UpdateWirePieces;
                _canvasController = ApplicationController.Instance.CanvasController;

                SettingsManager.Settings.ShowCurrent.PropertyChanged += ShowDebugValue;
                SettingsManager.Settings.ShowVoltage.PropertyChanged += ShowDebugValue;
                SettingsManager.PropertyChanged += SettingsManager_PropertyChanged;
            }
        }

        private void OnDisable()
        {
            SettingsManager.Settings.ShowCurrent.PropertyChanged -= ShowDebugValue;
            SettingsManager.Settings.ShowVoltage.PropertyChanged -= ShowDebugValue;
            SettingsManager.PropertyChanged -= SettingsManager_PropertyChanged;
        }

        private void UpdateVisuals()
        {
            foreach (var wirePiece in _wirePieces)
            {
                var gridCell = _grid.GetGridCell((int)wirePiece.PositionOnGrid.x, (int)wirePiece.PositionOnGrid.y);
                wirePiece.transform.position = gridCell.CenterInScreenCoord;
                wirePiece.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(wirePiece.GridSize.x * _grid.CellSize, wirePiece.GridSize.y * _grid.CellSize);

                wirePiece.UpdateCells();
            }
        }

        private void SettingsManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowDebugValue(sender, null);
        }

        private void ShowDebugValue(object sender, SettingChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                if (sender == SettingsManager.Settings.ShowCurrent)
                {
                    SettingsManager.Settings.ShowVoltage.Value = !e.NewValue;
                }
                else if (sender == SettingsManager.Settings.ShowVoltage)
                {
                    SettingsManager.Settings.ShowCurrent.Value = !e.NewValue;
                }
            }

            var value = SettingsManager.Settings.ShowCurrent.Value || SettingsManager.Settings.ShowVoltage.Value;
            foreach (var wirePiece in _wirePieces)
            {
                wirePiece.ShowDebugValue(value);
            }
        }

        public void UpdateWirePieces()
        {
            foreach (var wirePiece in _wirePieces)
            {
                wirePiece.UpdateCells();

                UpdateVisuals();

                wirePiece.UpdateConnections();
                wirePiece.LoadConnectedCharacters();
            }
        }

        public void StartNewWire(GridCell gridCell)
        {
            if (gridCell && !gridCell.IsOccupied)
            {
                AddWirePiece(gridCell);
                UpdateWirePieces();
            }
        }

        public void LoadSaveData(List<WirePieceData> data)
        {
            data.ForEach(d =>
            {
                var gridPos = d.DrawableData.PositionOnGrid;
                var gridCell = _grid.GetGridCell((int)gridPos.x, (int)gridPos.y);
                gridCell.ShowDebugValue(SettingsManager.Settings.ShowVoltage.Value || SettingsManager.Settings.ShowCurrent.Value);

                // Instantiate new wire piece
                var newWirePieceObject = Instantiate(WirePiecePrefab, _canvasController.WirePieceContainer, false);
                var newWirePiece = newWirePieceObject.GetComponent<WirePiece>();

                newWirePieceObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(newWirePiece.GridSize.x * _grid.CellSize, newWirePiece.GridSize.y * _grid.CellSize);
                newWirePieceObject.transform.position = gridCell.CenterInScreenCoord;
                newWirePiece.PositionOnGrid = gridCell.GridPos;
                newWirePiece.IsBridge = d.IsBridge;
                newWirePiece.Voltage = float.NaN;
                newWirePiece.Current = float.NaN;

                _grid.SetGridCells(gridCell.CenterInScreenCoord, newWirePiece);
                _wirePieces.Add(newWirePiece);

                foreach (var connection in d.Connections)
                {
                    newWirePiece.AddConnectionOnGrid(connection.Key, connection.Value.GetVector2());
                }

                if (GameState.CurrentState == GameStates.Game)
                {
                    newWirePiece.CanBeSelected = false;
                    newWirePiece.CanBeMoved = false;
                    newWirePiece.CanBeRotated = false;
                    newWirePiece.Cells.ForEach(c => c.SetNonEditable(true));
                }
            });

            UpdateWirePieces();
        }

        public void AddWirePiece(GridCell gridCell)
        {
            if (GameState.CurrentState == GameStates.Game)
            {
                var currLevel = ApplicationController.Instance.LevelController.CurrentLevel;
                if (currLevel.MaxWire == 0)
                    return;

                if (currLevel.MaxWire != -1)
                    currLevel.MaxWire -= 1;
            }

            var newWirePieceObject = Instantiate(WirePiecePrefab, _canvasController.WirePieceContainer, false);
            var newWirePiece = newWirePieceObject.GetComponent<WirePiece>();

            newWirePieceObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(newWirePiece.GridSize.x * _grid.CellSize, newWirePiece.GridSize.y * _grid.CellSize);
            newWirePieceObject.transform.position = gridCell.CenterInScreenCoord;
            newWirePiece.PositionOnGrid = gridCell.GridPos;
            _grid.SetGridCells(gridCell.CenterInScreenCoord, newWirePiece);
            newWirePiece.Voltage = float.NaN;
            newWirePiece.Current = float.NaN;

            ConnectToPrevious(newWirePiece);

            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                var neighbour = _grid.GetNeighbourGridCell(gridCell, dir);
                if (neighbour != null)
                {
                    var drawable = neighbour.DrawableBase;
                    if (drawable != null && drawable.Type != DrawableBase.DrawableType.Wire)
                    {
                        TryConnectToCharacter(neighbour, drawable as CharacterBase, false);
                    }
                }
            }
        }

        public void CloseLoop(WirePiece piece)
        {
            if (PreviousSelectedWirePiece != null && PreviousSelectedWirePiece != piece)
                ConnectToPrevious(piece);

            PreviousSelectedWirePiece = null;
        }

        private void ConnectToPrevious(WirePiece piece)
        {
            if (PreviousSelectedWirePiece != null)
            {
                var posDiff = piece.PositionOnGrid - PreviousSelectedWirePiece.PositionOnGrid;
                posDiff.Normalize();

                Direction direction = Direction.None;
                if (posDiff == Vector2.right)
                    direction = Direction.Right;
                else if (posDiff == Vector2.left)
                    direction = Direction.Left;
                else if (posDiff == Vector2.down)
                    direction = Direction.Top;
                else if (posDiff == Vector2.up)
                    direction = Direction.Bottom;

                if (direction != Direction.None)
                {
                    PreviousSelectedWirePiece.AddConnectionOnGrid(direction, piece.PositionOnGrid);
                    piece.AddConnectionOnGrid(direction.GetOpposite(), PreviousSelectedWirePiece.PositionOnGrid);

                    PreviousSelectedWirePiece.UpdateConnections();
                    piece.UpdateConnections();
                }
            }

            _grid.SetGridCells(piece.transform.position, piece);

            PreviousSelectedWirePiece = piece;
            _wirePieces.Add(piece);

            var gridCell = _grid.GetGridCell(piece.transform.position);
            gridCell.ShowDebugValue(SettingsManager.Settings.ShowVoltage.Value || SettingsManager.Settings.ShowCurrent.Value);
        }

        public void DeleteWirePiece(WirePiece piece)
        {
            if (GameState.CurrentState == GameStates.Game)
            {
                var currLevel = ApplicationController.Instance.LevelController.CurrentLevel;
                if (currLevel.MaxWire != -1)
                    currLevel.MaxWire += 1;
            }

            _wirePieces.Remove(piece);

            // If piece is bridge, search endpoint of connection it bridges and remove the connection
            if (piece.IsBridge)
            {
                var direction = piece.WireType == WirePiece.VisualType.HorizontalBridge ? Vector2.left : Vector2.up;
                var gridPos = piece.PositionOnGrid;
                var gridPosToCheck = gridPos + direction;
                var gridCellToCheck = _grid.GetGridCell((int)gridPosToCheck.x, (int)gridPosToCheck.y);

                var connectionStartFound = false;
                while (gridCellToCheck != null && !connectionStartFound)
                {
                    var drawable = gridCellToCheck.DrawableBase;
                    if (drawable.Type == DrawableBase.DrawableType.Wire)
                    {
                        var wirePiece = drawable as WirePiece;
                        if (wirePiece.IsBridge == false)
                        {
                            connectionStartFound = true;
                            var connection = wirePiece.GetConnection(direction.GetDirection().GetOpposite());
                            wirePiece.RemoveConnection(direction.GetDirection().GetOpposite());

                            // Reverse remove connection
                            if (connection.Type == DrawableBase.DrawableType.Wire)
                            {
                                var connectedWire = connection as WirePiece;
                                connectedWire.RemoveConnection(direction.GetDirection());
                            }
                            else
                            {
                                var connectedChar = connection as CharacterBase;
                                connectedChar.RemoveConnection(wirePiece);
                            }
                        }
                    }
                    else
                    {
                        direction *= -1;
                    }

                    gridPosToCheck += direction;
                    gridCellToCheck = _grid.GetGridCell((int)gridPosToCheck.x, (int)gridPosToCheck.y);
                }
            }

            ReverseRemoveConnections(piece);

            piece.Destroy();
        }

        public void ReverseRemoveConnections(WirePiece piece)
        {
            var connections = piece.Connections;
            foreach (var connection in connections)
            {
                // Let connections know about removal
                var drawable = connection.Value;
                if (drawable.Type == DrawableBase.DrawableType.Wire)
                {
                    var wire = drawable as WirePiece;
                    wire.RemoveConnection(connection.Key.GetOpposite());
                }
                else
                {
                    var character = drawable as CharacterBase;
                    character.RemoveConnection(piece);
                }
            }
        }

        public bool TryConnectToCharacterOverBridge(GridCell cell, GridCell bridge, CharacterBase character, bool userConnection = true)
        {
            if (character.CanConnectViaCell(bridge))
            {
                var posDiff = cell.GridPos - bridge.GridPos;
                posDiff.Normalize();

                Direction direction = Direction.None;
                if (posDiff == Vector2.right)
                    direction = Direction.Right;
                else if (posDiff == Vector2.left)
                    direction = Direction.Left;
                else if (posDiff == Vector2.down)
                    direction = Direction.Top;
                else if (posDiff == Vector2.up)
                    direction = Direction.Bottom;

                if (direction != Direction.None)
                {
                    PreviousSelectedWirePiece.AddConnectionOnGrid(direction, character.PositionOnGrid);
                    PreviousSelectedWirePiece.UpdateConnections();

                    character.AddConnection(PreviousSelectedWirePiece);
                }

                if (userConnection)
                    PreviousSelectedWirePiece = null;

                return true;
            }

            if (userConnection)
                PreviousSelectedWirePiece = null;

            return false;
        }

        public void TryConnectToCharacter(GridCell cell, CharacterBase character, bool userConnection = true)
        {
            var previousGridCell = _grid.GetGridCell((int)PreviousSelectedWirePiece.PositionOnGrid.x, (int)PreviousSelectedWirePiece.PositionOnGrid.y);
            TryConnectToCharacterOverBridge(cell, previousGridCell, character, userConnection);
        }

        public void TryConnectWireFromCharacter(GridCell batteryCell, GridCell wireCell, CharacterBase character)
        {
            if (character.CanConnectViaCell(wireCell))
            {
                var posDiff = batteryCell.GridPos - wireCell.GridPos;
                posDiff.Normalize();

                Direction direction = Direction.None;
                if (posDiff == Vector2.right)
                    direction = Direction.Right;
                else if (posDiff == Vector2.left)
                    direction = Direction.Left;
                else if (posDiff == Vector2.down)
                    direction = Direction.Top;
                else if (posDiff == Vector2.up)
                    direction = Direction.Bottom;

                if (direction != Direction.None)
                {
                    var wirePiece = wireCell.DrawableBase as WirePiece;
                    wirePiece.AddConnectionOnGrid(direction, character.PositionOnGrid);
                    wirePiece.UpdateConnections();

                    character.AddConnection(wirePiece);
                }
            }
        }

        public bool AllWirePiecesHaveVoltage()
        {
            foreach (var wirePiece in WirePieces)
            {
                if (float.IsNaN(wirePiece.Voltage))
                    return false;
            }

            return true;
        }

        public void ClearAllWirePieces()
        {
            foreach (var piece in _wirePieces)
            {
                piece.Current = float.NaN;
                piece.Voltage = float.NaN;
                piece.ClearCells();
                Destroy(piece.gameObject);
            }

            _wirePieces.Clear();
        }

        public void ResetAllWirePieces()
        {
            foreach (var piece in _wirePieces)
            {
                piece.Voltage = float.NaN;
                piece.Current = float.NaN;
                piece.DebugColor = Color.white;
            }
        }

        public void ResetColor()
        {
            foreach (var piece in _wirePieces)
            {
                piece.DebugColor = Color.white;
            }
        }
    }
}