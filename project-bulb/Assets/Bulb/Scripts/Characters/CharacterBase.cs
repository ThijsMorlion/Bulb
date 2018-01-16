using System.Collections.Generic;
using Bulb.Visuals.Grid;
using UnityEngine;
using Bulb.Controllers;
using Bulb.Characters.Wire;
using System;
using Bulb.Data;
using Bulb.Electricity;
using Bulb.Core;

namespace Bulb.Characters
{
    public class CharacterBase : DrawableBase
    {
        public enum CharacterState
        {
            None,
            Neutral,
            Happy,
            Dead,
            Open,
            Closed
        }

        [Header("Character Properties")]
        public string Name;
        public CharacterState State;
        public bool IsLinear; 
        public float Resistance;
        [NonSerialized]
        public float Current;
        public float Power;
        public Texture2D Icon;
        public Texture2D DeadImage;
        public ConnectionPole[] Connections;

        public Vector2[] ConnectionPoints;

        private List<WirePiece> _connections = new List<WirePiece>();
        private Dictionary<Pole, GridCell> _connectionCells = new Dictionary<Pole, GridCell>();

        public virtual void Reset()
        {
            PowerOn(0f);
            Current = float.NaN;
            SetImage(Icon, Color.white);
        }

        public void RemoveAllConnections()
        {
            // Break all existing connections
            foreach (var connection in _connections)
            {
                connection.RemoveConnection(this);
            }

            _connections.Clear();
        }

        public void InitializeConnectionPoints()
        {
            foreach (var connPole in Connections)
            {
                var connTransform = connPole.GetComponent<RectTransform>();
                var currAnchoredPos = connTransform.anchoredPosition;
                var normalizedAnchordedPos = new Vector2()
                {
                    x = currAnchoredPos.x / connTransform.sizeDelta.x,
                    y = currAnchoredPos.y / connTransform.sizeDelta.y
                };

                connTransform.anchoredPosition = normalizedAnchordedPos * _grid.CellSize;
                connTransform.sizeDelta = new Vector2(_grid.CellSize, _grid.CellSize);
            }
        }

        public void VerifyConnections()
        {
            UpdateConnectionPoints();

            var invalidConnections = new List<WirePiece>();
            foreach (var wirePiece in _connections)
            {
                var isConnected = false;
                foreach (var pair in _connectionCells)
                {
                    if (pair.Value.GridPos == wirePiece.PositionOnGrid)
                    {
                        isConnected = true;
                        break;
                    }
                }

                if (!isConnected)
                {
                    invalidConnections.Add(wirePiece);
                }
            }

            _connections.RemoveAll(c => invalidConnections.Contains(c));
        }

        public void UpdateConnectionPoints()
        {
            _connectionCells.Clear();

            foreach (var connPole in Connections)
            {
                var connTransform = connPole.GetComponent<RectTransform>();
                var gridCell = _grid.GetGridCell(connTransform.position);
                if (gridCell != null)
                {
                    _connectionCells.Add(connPole.Pole, gridCell);
                }
            }
        }

        public bool CanConnectViaCell(GridCell cell)
        {
            return _connectionCells.ContainsValue(cell);
        }

        public void AddConnection(WirePiece piece)
        {
            if(GetConnectedPole(piece) != Pole.None && !_connections.Contains(piece))
                _connections.Add(piece);
        }

        public List<WirePiece> GetAllConnections()
        {
            return _connections;
        }

        public Pole GetConnectedPole(WirePiece piece)
        {
            foreach (var connection in _connectionCells)
            {
                if (connection.Value.DrawableBase == piece)
                {
                    return connection.Key;
                }
            }

            return Pole.None;
        }

        public WirePiece GetOppositePole(Pole pole)
        {
            var otherPole = pole == Pole.Negative ? Pole.Positive : Pole.Negative;
            return GetConnection(otherPole);
        }

        public WirePiece GetConnection(Pole pole)
        {
            if (_connectionCells.ContainsKey(pole))
            {
                return _connectionCells[pole].DrawableBase as WirePiece;
            }

            return null;
        }

        public WirePiece GetConnectionViaDirection(Direction direction)
        {
            foreach(var conn in _connections)
            {
                if ((conn.PositionOnGrid - PositionOnGrid).GetDirection() == direction)
                    return conn;
            }

            return null;
        }

        public float GetPotentialDifference()
        {
            var posPoleWirePiece = GetConnection(Pole.Positive);
            var positivePoleVoltage = posPoleWirePiece != null ? posPoleWirePiece.Voltage : float.NaN;

            var negPoleWirePiece = GetConnection(Pole.Negative);
            var negativePoleVoltage = negPoleWirePiece != null ? negPoleWirePiece.Voltage : float.NaN;

            if(float.IsNaN(positivePoleVoltage) == false && float.IsNaN(negativePoleVoltage) == false)
            {
                var diff = positivePoleVoltage - negativePoleVoltage;
                return diff;
            }

            return float.NaN;
        }

        public void RemoveConnection(WirePiece piece)
        {
            _connections.Remove(piece);
        }

        public virtual void EvaluatePotentialDifference()
        {

        }

        public virtual void EvaluatePower()
        {
            if (_connections.Count == 2)
            {
                var firstConnection = _connections[0];
                var secondConnection = _connections[1];

                var firstPotDiff = firstConnection.Voltage;
                var secPotDiff = secondConnection.Voltage;
                var potentialDifference = Mathf.Abs(firstPotDiff - secPotDiff);

                var power = Current * potentialDifference;

                PowerOn(power);
            }
            else
            {
                PowerOn(0);
            }
        }

        public virtual void PowerOn(float value)
        {
            if (value > 0)
            {
                var percentage = Mathf.Clamp01(value / Power);
                DebugColor = new Color(1, 1, 1 - percentage, 1); 
            }
            else
            {
                DebugColor = new Color(1, 1, 1, 1);
            }
        }

        protected override void Update()
        {
            base.Update();

            var debugController = ApplicationController.Instance.DebugController;
            if (debugController.IsDebugMode)
            {
                ClearDebugPoints();
            }
        }

        private void ClearDebugPoints()
        {
            var debugController = ApplicationController.Instance.DebugController;
            for (var i = 0; i < ConnectionPoints.Length; ++i)
            {
                debugController.DeleteDebugPoint(string.Format("{0} | Connection: {1}", GetInstanceID(), i));
            }
        }

        public new CharacterData GetSaveData()
        {
            var drawableData = base.GetSaveData();
            var data = new CharacterData()
            {
                State = State,
                GridCellPositions = drawableData.GridCellPositions,
                PositionOnGrid = drawableData.PositionOnGrid,
                Rotation = drawableData.Rotation,
                Type = drawableData.Type
            };

            return data;
        }
    }
}