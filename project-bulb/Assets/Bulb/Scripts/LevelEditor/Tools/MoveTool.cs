using Bulb.Characters;
using Bulb.Characters.Wire;
using Bulb.Controllers;
using Bulb.Visuals.Grid;
using System.Collections.Generic;
using UnityEngine;

namespace Bulb.LevelEditor.Tools
{
    public static class MoveTool
    {
        private static List<DrawableBase> _selection;
        private static Dictionary<DrawableBase, Vector2> _oldPositions = new Dictionary<DrawableBase, Vector2>();

        private static BulbGrid _grid;
        private static List<bool> _invalidPositions = new List<bool>();
        private static SelectionController _selectionController;

        private static GridCell _startDragCell;
        private static DrawableBase _moveHandle;
        private static Vector2 _snapDiff = Vector2.zero;
        private static bool _isMoving = false;

        public static void Init(BulbGrid grid)
        {
            _grid = grid;
            _selectionController = ApplicationController.Instance.SelectionController;
        }

        public static void StartMove(DrawableBase moveHandle, Vector2 startDragPos, List<DrawableBase> drawables)
        {
            _selection = drawables;
            _startDragCell = _grid.GetGridCell(moveHandle.transform.position);
            _moveHandle = moveHandle;
            _snapDiff = Vector2.zero;

            drawables.ForEach(d =>
            {
                d.transform.SetAsLastSibling();
                d.ClearCells();
                _oldPositions.Add(d, d.ScreenPivot);

                if(d.Type != DrawableBase.DrawableType.Wire)
                {
                    var character = d as CharacterBase;
                    character.PowerOn(0);
                }
            });

            _selectionController.ClearSelection();
            _isMoving = true;
        }

        public static void UpdateMove(Vector2 diff)
        {
            if (_isMoving)
            {
                _invalidPositions.Clear();

                foreach (var s in _selection)
                {
                    var debugController = ApplicationController.Instance.DebugController;

                    var snapPos = Vector2.zero;

                    var validPosition = true;
                    if (s != _moveHandle)
                    {
                        validPosition = _grid.TryGetSnapPosition(_oldPositions[s] + _snapDiff + diff, out snapPos, s);
                    }
                    else
                    {
                        validPosition = _grid.TryGetSnapPosition(_startDragCell.CenterInScreenCoord + diff, out snapPos, s);
                    }

                    s.gameObject.transform.position = snapPos;
                    s.Image.color = validPosition == true ? Color.white : Color.red;

                    if (!validPosition)
                        _invalidPositions.Add(validPosition);
                } 
            }
        }

        public static void StopMove(Vector2 diff)
        {
            if (_isMoving)
            {
                var snapPosition = Vector2.zero;
                if (_invalidPositions.Count == 0)
                {
                    _selection.ForEach(s =>
                    {
                        if (s != _moveHandle)
                            _grid.TryGetSnapPosition(_oldPositions[s] + _snapDiff + diff, out snapPosition, s);
                        else
                            _grid.TryGetSnapPosition(_startDragCell.CenterInScreenCoord + diff, out snapPosition, s);

                        SetDrawablePosition(s, snapPosition);

                        if (s.Type != DrawableBase.DrawableType.Wire)
                        {
                            var character = (CharacterBase)s;
                            character.VerifyConnections();
                        }

                        var debugController = ApplicationController.Instance.DebugController;
                        debugController.DeleteDebugPoint(string.Format("Movetool: ScreenPos") + s.GetInstanceID());
                    });
                }
                else
                {
                    _selection.ForEach(s =>
                    {
                        snapPosition = _oldPositions[s];
                        SetDrawablePosition(s, snapPosition);

                        var debugController = ApplicationController.Instance.DebugController;
                        debugController.DeleteDebugPoint(string.Format("Movetool: ScreenPos") + s.GetInstanceID());
                    });
                }

                _invalidPositions.Clear();
                _oldPositions.Clear();
                _isMoving = false;

                ApplicationController.Instance.WireController.UpdateWirePieces();
            }
        }

        private static void SetDrawablePosition(DrawableBase s, Vector2 snapPosition)
        {
            var currGridPos = s.PositionOnGrid;

            s.GetComponent<RectTransform>().position = snapPosition;
            _grid.SetGridCells(snapPosition, s);
            s.Image.color = Color.white;

            var newGridPos = s.PositionOnGrid;
            var diff = newGridPos - currGridPos;

            if (s.GetType().IsSubclassOf(typeof(CharacterBase)))
            {
                var charBase = (CharacterBase)s;
                charBase.UpdateConnectionPoints();
            }
            else if (s.GetType() == typeof(WirePiece))
            {
                var wire = (WirePiece)s;
                wire.MoveConnectionsOnGrid(diff);
            }

            _selectionController.AddToSelection(s);
        }
    }
}