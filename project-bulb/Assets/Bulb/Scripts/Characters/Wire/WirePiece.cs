using System.Collections.Generic;
using UnityEngine;
using Bulb.Visuals.Grid;
using System.Linq;
using Bulb.Core;
using Bulb.Data;
using System;

namespace Bulb.Characters.Wire
{
    public class WirePiece : DrawableBase
    {
        public Texture2D StraightWire;
        public Texture2D CornerWire;
        public Texture2D WireEnd;
        public Texture2D WireT;
        public Texture2D WireX;
        public Texture2D Bridge;
        public Texture2D NoConnections;

        public VisualType WireType { get; private set; }
        public enum VisualType
        {
            Horizontal,
            Vertical,
            HorizontalBridge,
            VerticalBridge,
            CornerBottomLeft,
            CornerBottomRight,
            CornerTopLeft,
            CornerTopRight,
            IntersectionT,
            IntersectionX,
            End,
            NoConnections,
            None
        }

        private Direction _directionFlag;
        public Dictionary<Direction, DrawableBase> Connections { get; private set; }
        public Dictionary<Direction, Vector2> ConnectedGridPositions { get; set; }

        public bool IsBridge { get; set; }

        private float _current = float.NaN;
        public float Current
        {
            get
            {
                return _current;
            }

            set
            {
                if (_current != value)
                {
                    _current = value;

                    if (SettingsManager.Settings.ShowCurrent.Value)
                        SetDebugValue(value.FormatCurrent());
                }
            }
        }

        private float _voltage = float.NaN;
        public float Voltage
        {
            get
            {
                return _voltage;
            }

            set
            {
                if (_voltage != value)
                {
                    _voltage = value;

                    if (SettingsManager.Settings.ShowVoltage.Value)
                        SetDebugValue(value.FormatVoltage());
                }
            }
        }

        private float _previousAngle = -1f;
        private VisualType _previousWireType = VisualType.None;

        protected override void Awake()
        {
            base.Awake();
            Connections = new Dictionary<Direction, DrawableBase>();
            ConnectedGridPositions = new Dictionary<Direction, Vector2>();
        }

        public void ShowDebugValue(bool value)
        {
            var gridCell = _grid.GetGridCell((int)PositionOnGrid.x, (int)PositionOnGrid.y);
            gridCell.ShowDebugValue(value);

            if (SettingsManager.Settings.ShowVoltage.Value)
                gridCell.SetDebugValue(Voltage.FormatVoltage());
            else if (SettingsManager.Settings.ShowCurrent.Value)
                gridCell.SetDebugValue(Current.FormatCurrent());
        }

        private void SetDebugValue(string value)
        {
            var gridCell = _grid.GetGridCell((int)PositionOnGrid.x, (int)PositionOnGrid.y);
            gridCell.SetDebugValue(value);
        }

        public void UpdateConnections()
        {
            Connections.Clear();
            Connections = new Dictionary<Direction, DrawableBase>();

            var keysToBeRemoved = new List<Direction>();
            foreach (var connection in ConnectedGridPositions)
            {
                var gridPos = new Vector2(connection.Value.x, connection.Value.y);
                var gridCell = _grid.GetGridCell((int)gridPos.x, (int)gridPos.y);
                if (gridCell != null && gridCell.DrawableBase != null)
                {
                    Connections.Add(connection.Key, gridCell.DrawableBase);
                }
                else
                {
                    keysToBeRemoved.Add(connection.Key);
                    Debug.LogWarningFormat("{0} | No GridCell or DrawableBase found at position {1}", this, gridPos);
                }
            }

            UpdateWireType();
            keysToBeRemoved.ForEach(k => RemoveConnection(k));
        }

        public void MoveConnectionsOnGrid(Vector2 diff)
        {
            var newConnectionPositions = new Dictionary<Direction, Vector2>();
            foreach (var connection in ConnectedGridPositions)
            {
                newConnectionPositions.Add(connection.Key, connection.Value + diff);
            }

            ConnectedGridPositions = newConnectionPositions;
        }

        public void LoadConnectedCharacters()
        {
            foreach (var connection in Connections)
            {
                if (connection.Value.GetType().IsSubclassOf(typeof(CharacterBase)))
                {
                    var character = (CharacterBase)connection.Value;
                    character.AddConnection(this);
                }
            }
        }

        public void AddConnectionOnGrid(Direction direction, Vector2 gridPos)
        {
            if (!ConnectedGridPositions.ContainsKey(direction))
            {
                ConnectedGridPositions.Add(direction, gridPos);

                _directionFlag |= direction;

                UpdateWireType();
            }
        }

        public void RemoveConnection(Direction direction)
        {
            if (ConnectedGridPositions.ContainsKey(direction))
            {
                var connection = GetConnection(direction);

                ConnectedGridPositions.Remove(direction);

                if (connection)
                    ClearBridges(Connections[direction]);

                _directionFlag ^= direction;

                UpdateWireType();
                UpdateConnections();
            }
        }

        public void RemoveConnection(CharacterBase character)
        {
            var connection = Connections.Where(c => c.Value == character).Select(p => p.Key).ToList();
            if (connection.Count() == 1)
            {
                RemoveConnection(connection[0]);
            }

            ClearBridges(character);
        }

        public void ClearBridges(DrawableBase drawable)
        {
            // If distance between connection and wirepiece is bigger than 1 cell, there is a bridge between them
            var diff = drawable.PositionOnGrid - PositionOnGrid;
            if (diff.magnitude > 1)
            {
                var normalizedDiff = diff.normalized;
                var nextBridgeDiff = normalizedDiff;
                while (nextBridgeDiff != diff)
                {
                    var bridgeCell = _grid.GetGridCell((int)(PositionOnGrid.x + nextBridgeDiff.x), (int)(PositionOnGrid.y + nextBridgeDiff.y));
                    if (bridgeCell != null)
                    {
                        var bridgeCellDrawable = bridgeCell.DrawableBase;
                        if (bridgeCellDrawable != null && bridgeCellDrawable.Type == DrawableType.Wire)
                        {
                            var bridgedWire = bridgeCellDrawable as WirePiece;
                            bridgedWire.IsBridge = false;
                        } 
                    }

                    nextBridgeDiff += normalizedDiff;
                }
            }
        }

        public DrawableBase GetConnection(Direction direction)
        {
            if (Connections.ContainsKey(direction))
                return Connections[direction];

            return null;
        }

        public bool IsConnectedTo(DrawableBase drawable)
        {
            foreach (var connection in Connections)
            {
                if (connection.Value == drawable)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Update()
        {
            base.Update();

            UpdateWireType();

            if (_previousWireType != WireType)
            {
                switch (WireType)
                {
                    case VisualType.Horizontal:
                    case VisualType.Vertical:
                        SetImage(StraightWire, Color.white);
                        break;
                    case VisualType.IntersectionT:
                        SetImage(WireT, Color.white);
                        break;
                    case VisualType.IntersectionX:
                        SetImage(WireX, Color.white);
                        break;
                    case VisualType.End:
                        SetImage(WireEnd, Color.white);
                        break;
                    case VisualType.HorizontalBridge:
                    case VisualType.VerticalBridge:
                        SetImage(Bridge, Color.white);
                        break;
                    case VisualType.NoConnections:
                        SetImage(NoConnections, Color.white);
                        break;
                    default:
                        SetImage(CornerWire, Color.white);
                        break;
                }

                _previousWireType = WireType;
            }

            if (_previousAngle != Rotation)
            {
                _previousAngle = Rotation;
                SetRotationAroundPivot(Rotation);
            }
        }

        private void UpdateWireType()
        {
            if (IsBridge)
            {
                switch (_directionFlag)
                {
                    case Direction.Left | Direction.Right:
                        WireType = VisualType.VerticalBridge;
                        Rotation = 0f;
                        break;
                    case Direction.Top | Direction.Bottom:
                        WireType = VisualType.HorizontalBridge;
                        Rotation = 90f;
                        break;
                }
            }
            else
            {
                // If number of connections is equal to 1, the wirepiece is an end
                if (Connections.Count == 0)
                {
                    WireType = VisualType.NoConnections;
                }
                else if (Connections.Count == 1)
                {
                    WireType = VisualType.End;

                    KeyValuePair<Direction, DrawableBase> connection;
                    if (Connections.TryFirstOrDefault(out connection))
                    {
                        switch (Connections.First().Key)
                        {
                            case Direction.Top:
                                Rotation = 180f;
                                break;
                            case Direction.Left:
                                Rotation = 270f;
                                break;
                            case Direction.Right:
                                Rotation = 90f;
                                break;
                            case Direction.Bottom:
                                Rotation = 0f;
                                break;
                        }
                    }
                }
                // If number of connections is equal to 2, the wirepiece can be straight or bended
                else if (Connections.Count == 2)
                {
                    switch (_directionFlag)
                    {
                        case Direction.Left | Direction.Right:
                            WireType = VisualType.Horizontal;
                            Rotation = 90f;
                            break;
                        case Direction.Bottom | Direction.Top:
                            WireType = VisualType.Vertical;
                            Rotation = 0f;
                            break;
                        case Direction.Left | Direction.Bottom:
                            WireType = VisualType.CornerBottomLeft;
                            Rotation = 270f;
                            break;
                        case Direction.Right | Direction.Bottom:
                            WireType = VisualType.CornerBottomRight;
                            Rotation = 0f;
                            break;
                        case Direction.Left | Direction.Top:
                            WireType = VisualType.CornerTopLeft;
                            Rotation = 180f;
                            break;
                        case Direction.Right | Direction.Top:
                            WireType = VisualType.CornerTopRight;
                            Rotation = 90f;
                            break;
                    }
                }
                // If number of connections equals 3, the wirepiece is a T intersection
                else if (Connections.Count == 3)
                {
                    WireType = VisualType.IntersectionT;

                    if ((_directionFlag & Direction.Right) == 0)
                    {
                        Rotation = 180f;
                    }
                    else if ((_directionFlag & Direction.Top) == 0)
                    {
                        Rotation = 270f;
                    }
                    else if ((_directionFlag & Direction.Bottom) == 0)
                    {
                        Rotation = 90f;
                    }
                    else if ((_directionFlag & Direction.Left) == 0)
                    {
                        Rotation = 0f;
                    }
                }
                // If number of connections equals 4, the wirepiece is an X intersection
                else if (Connections.Count == 4)
                {
                    WireType = VisualType.IntersectionX;
                }
            }
        }

        public new WirePieceData GetSaveData()
        {
            var drawableData = base.GetSaveData();
            var wirePieceData = new WirePieceData()
            {
                IsBridge = IsBridge,
                DirectionFlag = _directionFlag,
                DrawableData = drawableData,
                Connections = new Dictionary<Direction, SerializableVector2>(),
                Type = WireType
            };

            foreach (var connection in Connections)
            {
                var gridPos = connection.Value.PositionOnGrid;
                wirePieceData.Connections.Add(connection.Key, new SerializableVector2(gridPos.x, gridPos.y));
            }

            return wirePieceData;
        }
    }
}