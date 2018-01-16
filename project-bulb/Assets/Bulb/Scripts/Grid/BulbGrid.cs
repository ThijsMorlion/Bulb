using Bulb.Characters;
using Bulb.Characters.Wire;
using Bulb.Controllers;
using Bulb.Core;
using Bulb.Data;
using Bulb.LevelEditor.Tools;
using Bulb.LevelEditor.Widgets;
using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Pointers;
using UnityEngine;
using UnityEngine.UI;
using Bulb.LevelEditor.Popups;
using UnityEngine.Events;
using Bulb.Electricity;
using Bulb.Game;
using Bulb.UI.Game;

namespace Bulb.Visuals.Grid
{
    [Flags]
    public enum Direction
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Top = 1 << 2,
        Bottom = 1 << 3
    }

    public class BulbGrid : MonoBehaviour
    {
        public static BulbGrid Instance;

        private GridMode _currentGridMode;
        public enum GridMode
        {
            Selection,
            DrawWire,
            Move,
            None
        }

        public Transform GridCellPrefab;

        public int NumberOfColumns;
        public int NumberOfRows;

        public float ScreenCellSize
        {
            get
            {
                return CellSize * transform.lossyScale.x;
            }
        }

        public float CellSize
        {
            get
            {
                return GetComponent<GridLayoutGroup>().cellSize.x;
            }

            set
            {
                _gridLayoutGroup.cellSize = new Vector2(value, value);

                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());

                if (OnGridChanged != null)
                    OnGridChanged();
            }
        }

        #region GRID_EVENTS
        public delegate void GridChanged();
        public event GridChanged OnGridChanged;

        public delegate void GridInitialized();
        public GridInitialized OnGridInitialized;
        #endregion

        #region VARIABLES_GRID_DIMENSIONS
        private Vector2 GridTopLeftCorner
        {
            get
            {
                var leftCorner = new Vector2()
                {
                    x = _rectTransform.position.x - (NumberOfColumns * CellSize * transform.lossyScale.x) / 2,
                    y = _rectTransform.position.y + (NumberOfRows * CellSize * transform.lossyScale.y) / 2
                };

                return leftCorner;
            }
        }
        private RectTransform _rectTransform;
        private Vector2 _gridScreenPos;
        private bool _gridCentered = true;
        #endregion

        #region VARIABLES_GRID_INTERACTIONS
        public GridModeToggleGroup GridModeToggleGroup;
        private TapGesture _tapGesture;
        private ScreenTransformGesture _transformGesture;
        private bool _isMoving = false;
        private bool _isScaling = false;
        private bool _startedOnCharacter = false;
        private GridCell _characterStartedGridCell = null;
        #endregion

        private GridLayoutGroup _gridLayoutGroup;
        private List<GridCell> _cells = new List<GridCell>();

        private void Awake()
        {
            Instance = this;

            if (GridCellPrefab == null)
            {
                Debug.LogFormat("{0} | No GridCellPrefab assigned!", this);
                return;
            }

            _rectTransform = GetComponent<RectTransform>();
            _gridLayoutGroup = GetComponent<GridLayoutGroup>();

            InitializeGrid();

            // Init editor tools
            MoveTool.Init(this);
            RotateTool.Init(this);
        }

        private void OnEnable()
        {
            _tapGesture = transform.GetOrAddComponent<TapGesture>();
            _tapGesture.Tapped += OnTapped;

            _transformGesture = transform.GetOrAddComponent<ScreenTransformGesture>();
            _transformGesture.TransformStarted += OnDragStarted;
            _transformGesture.Transformed += OnDragUpdate;
            _transformGesture.TransformCompleted += OnDragCompleted;
            _transformGesture.Cancelled += (object sender, EventArgs e) => Debug.Log("Cancelled transformgesture");

            ApplicationController.Instance.SelectionController.OnSelectionAreaDrawCompleted += OnSelectionAreaDrawCompleted;

            if (GridModeToggleGroup)
            {
                GridModeToggleGroup.OnToggleValueChanged += (GridMode mode) => { SetGridMode(mode); };
                SetGridMode(GridModeToggleGroup.CurrentDrawMode);
            }
        }

        private void OnDisable()
        {
            _tapGesture = transform.GetOrAddComponent<TapGesture>();
            _tapGesture.Tapped -= OnTapped;

            _transformGesture = transform.GetOrAddComponent<ScreenTransformGesture>();
            _transformGesture.TransformStarted -= OnDragStarted;
            _transformGesture.Transformed -= OnDragUpdate;
            _transformGesture.TransformCompleted -= OnDragCompleted;

            if (ApplicationController.Instance.SelectionController != null)
                ApplicationController.Instance.SelectionController.OnSelectionAreaDrawCompleted -= OnSelectionAreaDrawCompleted;

            if (GridModeToggleGroup)
            {
                GridModeToggleGroup.OnToggleValueChanged -= (GridMode mode) => { SetGridMode(mode); };
                SetGridMode(GridModeToggleGroup.CurrentDrawMode);
            }
        }

        #region DRAG LOGICS
        public void OnDragStarted(object sender, EventArgs e)
        {
            if (CurrentWalker.IsSimulating == false)
            {
                var selectionController = ApplicationController.Instance.SelectionController;
                var transformGesture = (ScreenTransformGesture)sender;
                var pointers = transformGesture.ActivePointers;
                if (pointers.Count == 1)
                {
                    switch (pointers[0].Buttons)
                    {
                        case Pointer.PointerButtonState.FirstButtonPressed:
                            switch (_currentGridMode)
                            {
                                case GridMode.DrawWire:
                                    {
                                        var wireController = ApplicationController.Instance.WireController;

                                        // CREATE NEW WIRE OBJECT
                                        var screenPos = transformGesture.ScreenPosition;
                                        var gridCell = GetGridCell(screenPos);
                                        if (gridCell)
                                        {
                                            if (gridCell.DrawableBase == null)
                                            {
                                                wireController.PreviousSelectedWirePiece = null;
                                                wireController.StartNewWire(gridCell);
                                            }
                                            else if (gridCell.DrawableBase.Type == DrawableBase.DrawableType.Wire)
                                            {
                                                var currLevel = ApplicationController.Instance.LevelController.CurrentLevel;
                                                var wirePiece = (WirePiece)gridCell.DrawableBase;
                                                wireController.PreviousSelectedWirePiece = wirePiece;

                                                if (GameState.CurrentState == GameStates.Game && (!currLevel.CanBranch && (wirePiece.WireType != WirePiece.VisualType.End && wirePiece.WireType != WirePiece.VisualType.NoConnections)))
                                                    return;
                                            }
                                            else
                                            {
                                                _startedOnCharacter = true;
                                                _characterStartedGridCell = gridCell;
                                            }
                                        }
                                        break;
                                    }
                                case GridMode.Selection:
                                    {
                                        selectionController.StartDrag(transformGesture.ScreenPosition);
                                        break;
                                    }
                                case GridMode.Move:
                                    {
                                        var gridCell = GetGridCell(transformGesture.ScreenPosition);

                                        if (gridCell)
                                        {
                                            var moveHandle = gridCell.DrawableBase;
                                            if (moveHandle && moveHandle.CanBeMoved)
                                            {
                                                if (!selectionController.IsPartOfSelection(moveHandle))
                                                {
                                                    selectionController.ClearSelection();
                                                    selectionController.AddToSelection(moveHandle);
                                                }

                                                selectionController.StartMove(moveHandle, transformGesture);
                                            }
                                        }
                                        break;
                                    }
                            }
                            break;
                        case Pointer.PointerButtonState.ThirdButtonPressed:
                            _isMoving = true;
                            break;
                    }
                }
                else if (pointers.Count == 2)
                {
                    selectionController.ClearSelection();
                    _isScaling = true;
                }
                else if (pointers.Count == 3)
                {
                    _isMoving = true;
                }
            }
        }

        public void OnDragUpdate(object sender, EventArgs e)
        {
            if (CurrentWalker.IsSimulating == false)
            {
                var selectionController = ApplicationController.Instance.SelectionController;
                var transformGesture = (ScreenTransformGesture)sender;
                if (_isMoving)
                {
                    var diff = transformGesture.ScreenPosition - transformGesture.PreviousScreenPosition;
                    MoveGrid(diff);

                }
                else if (_isScaling)
                {
                    ZoomGrid(transformGesture.DeltaScale);
                }
                else
                {
                    switch (_currentGridMode)
                    {
                        case GridMode.DrawWire:
                            {
                                var wireController = ApplicationController.Instance.WireController;

                                var screenPos = transformGesture.ScreenPosition;
                                var gridCell = GetGridCell(screenPos);
                                if (gridCell)
                                {
                                    var previousWirePiece = wireController.PreviousSelectedWirePiece;
                                    if (!_startedOnCharacter && previousWirePiece == null)
                                    {
                                        return;
                                    }

                                    if (previousWirePiece)
                                    {
                                        var diff = previousWirePiece.PositionOnGrid - gridCell.GridPos;
                                        if (diff.magnitude > 1)
                                        {
                                            return;
                                        }
                                    }

                                    if (gridCell.DrawableBase == null)
                                    {
                                        if (wireController.PreviousSelectedWirePiece != null)
                                        {
                                            if (wireController.PreviousSelectedWirePiece.WireType != WirePiece.VisualType.End && GameState.CurrentState == GameStates.Game)
                                            {
                                                var currLevel = ApplicationController.Instance.LevelController.CurrentLevel;
                                                if (!currLevel.CanBranch)
                                                {
                                                    transformGesture.Cancel();
                                                    return;
                                                }

                                                if (!currLevel.CanSnap)
                                                    return;
                                            }
                                        }

                                        wireController.AddWirePiece(gridCell);
                                    }
                                    else if (gridCell.DrawableBase.Type == DrawableBase.DrawableType.Wire)
                                    {
                                        var wirePiece = gridCell.DrawableBase as WirePiece;
                                        if (previousWirePiece != null && wirePiece != null)
                                        {
                                            if (previousWirePiece != wirePiece && !previousWirePiece.IsConnectedTo(wirePiece))
                                            {
                                                if (wirePiece.WireType == WirePiece.VisualType.Horizontal || wirePiece.WireType == WirePiece.VisualType.Vertical)
                                                {
                                                    transformGesture.Cancel();
                                                    TryBridgeWirePiece(wirePiece, previousWirePiece, 1);
                                                }
                                                else
                                                {
                                                    var currLevel = ApplicationController.Instance.LevelController.CurrentLevel;
                                                    if (!currLevel.CanSnap && ((wirePiece.WireType != WirePiece.VisualType.End && wirePiece.WireType != WirePiece.VisualType.NoConnections) && 
                                                                               (previousWirePiece.WireType != WirePiece.VisualType.End && previousWirePiece.WireType != WirePiece.VisualType.NoConnections)))
                                                    {
                                                        return;
                                                    }

                                                    wireController.CloseLoop(wirePiece);
                                                }
                                            }
                                            else if (previousWirePiece.IsConnectedTo(wirePiece) && previousWirePiece.CanBeSelected)
                                            {
                                                wireController.DeleteWirePiece(previousWirePiece);
                                                wireController.PreviousSelectedWirePiece = wirePiece;

                                                return;
                                            }
                                        }
                                        else if (_startedOnCharacter)
                                        {
                                            _startedOnCharacter = false;
                                            var drawable = _characterStartedGridCell.DrawableBase;
                                            wireController.TryConnectWireFromCharacter(_characterStartedGridCell, gridCell, drawable as CharacterBase);
                                            //foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                                            //{
                                            //    var neighbour = GetNeighbourGridCell(gridCell, dir);
                                            //    if (neighbour != null)
                                            //    {
                                            //        var drawable = neighbour.DrawableBase;
                                            //        if (drawable != null && drawable.Type == DrawableBase.DrawableType.Wire)
                                            //        {
                                            //            wireController.TryConnectWireFromCharacter(gridCell, neighbour, drawable as CharacterBase);
                                            //        }
                                            //    }
                                            //}
                                        }
                                    }
                                    else if (gridCell.DrawableBase.GetType().IsSubclassOf(typeof(CharacterBase)))
                                    {
                                        if (!_startedOnCharacter)
                                        {
                                            wireController.TryConnectToCharacter(gridCell, (CharacterBase)gridCell.DrawableBase);
                                            transformGesture.Cancel();
                                        }
                                    }
                                }
                                break;
                            }
                        case GridMode.Selection:
                            {
                                selectionController.Drag(transformGesture.ScreenPosition);
                                break;
                            }
                        case GridMode.Move:
                            {
                                selectionController.UpdateMove(transformGesture);
                                break;
                            }
                    }
                }
            }
        }

        private void TryBridgeWirePiece(WirePiece wirePiece, WirePiece previousWirePiece, int iteration)
        {
            var bridgePos = wirePiece.PositionOnGrid;
            var previousPos = previousWirePiece.PositionOnGrid;
            var wireController = ApplicationController.Instance.WireController;
            var diff = bridgePos - previousPos;

            diff.Normalize();
            if (diff.magnitude != 1)
                return;

            var continuePos = previousPos + diff * (iteration + 1);
            var continueCell = GetGridCell((int)continuePos.x, (int)continuePos.y);
            var bridgeCell = GetGridCell((int)bridgePos.x, (int)bridgePos.y);

            if (continueCell == null)
            {
                wireController.CloseLoop(wirePiece);
            }
            else
            {
                var bridgePopup = CreateBridgePopup.Instance as CreateBridgePopup;
                bridgePopup.TogglePopup(true);
                bridgePopup.SetScreenPosition(bridgeCell.CenterInScreenCoord);

                UnityAction CanceledAction = null;
                CanceledAction = () =>
                {
                    wirePiece.ClearBridges(previousWirePiece);
                    bridgePopup.OnPopupCanceled.RemoveListener(CanceledAction);
                    bridgePopup.OnCreateBridgeConfirmed.RemoveAllListeners();
                };
                bridgePopup.OnPopupCanceled.AddListener(CanceledAction);

                UnityAction<bool> ConfirmAction = null;
                ConfirmAction = (value) =>
                {
                    bridgePopup.OnPopupCanceled.RemoveListener(CanceledAction);

                    if (!value)
                    {
                        wireController.CloseLoop(wirePiece);
                    }
                    else
                    {
                        var continueDrawable = continueCell.DrawableBase;
                        if (continueDrawable == null)
                        {
                            wireController.AddWirePiece(continueCell);
                            wirePiece.IsBridge = value;
                            wireController.PreviousSelectedWirePiece = null;
                        }
                        else
                        {
                            if (continueDrawable.Type == DrawableBase.DrawableType.Wire)
                            {
                                var wireDrawable = continueDrawable as WirePiece;
                                wirePiece.IsBridge = value;

                                if (wireDrawable.WireType == WirePiece.VisualType.Horizontal || wireDrawable.WireType == WirePiece.VisualType.Vertical)
                                {
                                    TryBridgeWirePiece(wireDrawable, previousWirePiece, ++iteration);
                                }
                                else
                                {
                                    wireController.CloseLoop(continueDrawable as WirePiece);
                                }
                            }
                            else
                            {
                                var result = wireController.TryConnectToCharacterOverBridge(continueCell, bridgeCell, continueDrawable as CharacterBase);
                                wirePiece.IsBridge = result;

                                if (!result)
                                    wireController.CloseLoop(wirePiece);
                            }
                        }
                    }

                    bridgePopup.OnCreateBridgeConfirmed.RemoveListener(ConfirmAction);
                };

                bridgePopup.OnCreateBridgeConfirmed.AddListener(ConfirmAction);
            }
        }

        public void OnDragCompleted(object sender, EventArgs e)
        {
            if (CurrentWalker.IsSimulating == false)
            {
                var transformGesture = (ScreenTransformGesture)sender;
                if (_isMoving)
                {
                    _isMoving = false;

                }
                else if (_isScaling)
                {
                    _isScaling = false;
                }
                else
                {
                    var selectionController = ApplicationController.Instance.SelectionController;
                    switch (_currentGridMode)
                    {
                        case GridMode.DrawWire:
                            {
                                ApplicationController.Instance.WireController.PreviousSelectedWirePiece = null;
                                break;
                            }
                        case GridMode.Selection:
                            {
                                selectionController.EndDrag();
                                break;
                            }
                        case GridMode.Move:
                            {
                                selectionController.StopMove(transformGesture);
                                break;
                            }
                    }
                }
            }
        }
        #endregion

        #region TAP LOGICS
        public void OnTapped(object sender, EventArgs e)
        {
            if (CurrentWalker.IsSimulating == false)
            {
                if (GameState.CurrentState == GameStates.Editor)
                {
                    var selectionController = ApplicationController.Instance.SelectionController;
                    selectionController.ClearSelection();

                    var tapGesture = (TapGesture)sender;
                    var gridCell = GetGridCell(tapGesture.ScreenPosition);
                    if (gridCell)
                    {
                        if (gridCell.DrawableBase)
                            selectionController.AddToSelection(gridCell.DrawableBase);
                    }
                }
                else if (GameState.CurrentState == GameStates.Game)
                {
                    var tapGesture = (TapGesture)sender;
                    var gridCell = GetGridCell(tapGesture.ScreenPosition);
                    if (gridCell)
                    {
                        var drawable = gridCell.DrawableBase;
                        if (drawable)
                        {
                            if (drawable.CanBeSelected)
                            {
                                var selectionController = ApplicationController.Instance.SelectionController;
                                selectionController.ClearSelection();
                                selectionController.AddToSelection(drawable);

                                var actionWidget = ActionWidget.Instance;
                                actionWidget.Drawable = drawable;
                                actionWidget.TogglePopup(true);
                            }
                        }
                    }
                }
            }
            else
            {
                var tapGesture = (TapGesture)sender;
                var gridCell = GetGridCell(tapGesture.ScreenPosition);
                if (gridCell)
                {
                    var drawableBase = gridCell.DrawableBase;
                    if (drawableBase)
                    {
                        if (drawableBase.Type == DrawableBase.DrawableType.Switch)
                        {
                            var switchCharacter = (SwitchCharacter)drawableBase;
                            switchCharacter.ToggleSwitch();

                            var charController = ApplicationController.Instance.CharacterController;
                            charController.ResetAllButExcludedCharacters(new List<CharacterBase>() { switchCharacter });

                            var currentWalker = ApplicationController.Instance.CurrentWalker;
                            currentWalker.AnalyzeCircuit();
                        }
                    }
                }
            }
        }
        #endregion

        private void OnSelectionAreaDrawCompleted(RectTransform selectionAreaObject)
        {
            var selectionController = ApplicationController.Instance.SelectionController;
            foreach (var cell in _cells)
            {
                try
                {
                    // TODO: check on rect overlap instead of position, figure out how to do this correctly
                    if (RectTransformUtility.RectangleContainsScreenPoint(selectionAreaObject, cell.transform.position))
                    {
                        if (cell.DrawableBase != null)
                            selectionController.AddToSelection(cell.DrawableBase);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogFormat("{0} | OnSelectionAreaDrawCompleted: {1}", this, e.Message);
                }
            }
        }

        private void CenterGrid()
        {
            var editArea = ApplicationController.Instance.CanvasController.EditCanvas;
            transform.position = editArea.position;
        }

        private void Update()
        {
            if (_gridCentered)
            {
                CenterGrid();
            }
        }

        #region GRID_TRANSFORM_LOGIC
        public void ZoomGrid(float factor)
        {
            CellSize *= factor;
        }

        public void FillZoomGrid()
        {
            var canvasController = ApplicationController.Instance.CanvasController;

            var refValue = Mathf.Min(canvasController.EditCanvas.rect.width, canvasController.EditCanvas.rect.height);
            var divValue = refValue == canvasController.EditCanvas.rect.width ? NumberOfColumns : NumberOfRows;

            var filledGridcellSize = refValue * .8f / divValue;
            var zoomFactor = filledGridcellSize / CellSize;

            ZoomGrid(zoomFactor);
        }

        public void MoveGrid(Vector2 diff)
        {
            ApplicationController.Instance.SelectionController.ClearSelection();

            var newPos = (Vector2)transform.position + diff;
            transform.position = newPos;
            _gridCentered = false;

            if (OnGridChanged != null)
                OnGridChanged();
        }

        private void InitializeGrid()
        {
            _cells.ForEach(c => DestroyImmediate(c.gameObject));
            _cells.Clear();

            _gridLayoutGroup.constraintCount = NumberOfColumns;

            // Add cells
            for (var y = 0; y < NumberOfRows; ++y)
            {
                for (var x = 0; x < NumberOfColumns; ++x)
                {
                    AddGridCell();
                }
            }

            UpdateGridCellIndices();

            if (OnGridInitialized != null)
                OnGridInitialized();
        }

        private void ClearGridCells()
        {
            _cells.ForEach(c =>
            {
                c.Clear();
            });
        }

        private void ExtendGrid()
        {
            ApplicationController.Instance.SelectionController.ClearSelection();
            ClearGridCells();

            var newCellNumber = NumberOfColumns * NumberOfRows;
            var cellDiff = newCellNumber - _cells.Count;

            for (int i = 0; i < cellDiff; ++i)
                AddGridCell();

            UpdateGridCellIndices();
        }

        private void AddGridCell()
        {
            var newCell = Instantiate(GridCellPrefab, transform, false);
            var newCellScript = newCell.GetComponent<GridCell>();
            newCellScript.SetNonEditable(false);

            _cells.Add(newCellScript);
        }

        private void UpdateGridCellIndices()
        {
            // Clear cells
            _cells.Clear();

            // Update cell array
            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                var cell = child.GetComponent<GridCell>();
                var gridPos = GetCellRowColumn(i);

                cell.Init(gridPos);
                child.name = string.Format("Board Space ( x = {0}, y = {1})", gridPos.x, gridPos.y);

                _cells.Add(cell);
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.GetComponent<RectTransform>());

            if (OnGridChanged != null)
                OnGridChanged();
        }
        #endregion

        public void SetNumberOfColumns(int value)
        {
            if (value < NumberOfColumns)
                return;

            NumberOfColumns = value;
            _gridLayoutGroup.constraintCount = NumberOfColumns;
            ExtendGrid();
        }

        public void SetNumberOfRows(int value)
        {
            if (value < NumberOfRows)
                return;

            NumberOfRows = value;
            ExtendGrid();
        }

        public bool TryRotate(DrawableBase drawable, float angle)
        {
            var cells = drawable.Cells;
            if (angle != 0)
            {
                foreach (var cell in cells)
                {
                    var point = cell.CenterInScreenCoord;

                    var newCellPos = point.RotateAround(drawable.ScreenPivot, Quaternion.Euler(0, 0, angle));
                    var cellAfterRotation = GetGridCell(newCellPos);
                    if (cellAfterRotation == null)
                        return false;
                }
            }

            return true;
        }

        public bool TryGetSnapPosition(Vector2 screenPos, out Vector2 snapPos, DrawableBase drawable)
        {
            var gridPos = GetGridCellRowColumn(screenPos);
            if (!Single.IsNegativeInfinity(gridPos.x) && !Single.IsNegativeInfinity(gridPos.y))
            {
                var gridCell = GetGridCell(screenPos);
                if (gridCell)
                {
                    // Check if neighbouring cells are not occupied
                    if (AreaIsOccupied(gridPos, drawable))
                    {
                        //Debug.LogFormat("{0} | Area is occupied!", this);
                        snapPos = screenPos;
                        return false;
                    }

                    snapPos = gridCell.CenterInScreenCoord;
                    return true;
                }
            }

            snapPos = screenPos;
            return false;
        }

        private bool AreaIsOccupied(Vector2 gridPos, DrawableBase drawable)
        {
            var debugController = ApplicationController.Instance.DebugController;
            var isOccupied = false;

            for (var x = 0; x < drawable.GridSize.x; ++x)
            {
                for (var y = 0; y < drawable.GridSize.y; ++y)
                {
                    var currScreenPos = GetGridCell((int)gridPos.x, (int)gridPos.y).CenterInScreenCoord;
                    var posToCheck = new Vector2()
                    {
                        x = currScreenPos.x + (x * ScreenCellSize),
                        y = currScreenPos.y - (y * ScreenCellSize)
                    };

                    var gridCell = GetGridCell(posToCheck);
                    if (drawable.Rotation != 0)
                    {
                        var rotatedPos = posToCheck.RotateAround(drawable.ScreenPivot, Quaternion.Euler(0, 0, drawable.Rotation));
                        gridCell = GetGridCell(rotatedPos);
                    }

                    if (!gridCell || gridCell.IsOccupied)
                    {
                        return true;
                    }
                }
            }

            return isOccupied;
        }

        #region GRIDCELL_GETTERS
        private int GetGridCellIndex(int row, int column)
        {
            return (column * NumberOfColumns) + row;
        }

        private Vector2 GetCellRowColumn(int index)
        {
            var x = index % NumberOfColumns;
            var y = index / NumberOfColumns;

            return new Vector2(x, y);
        }

        public Vector2 GetGridCellRowColumn(Vector2 screenPos)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, screenPos))
            {
                var invertedScreenPos = new Vector2(screenPos.x, Screen.height - screenPos.y);

                _gridScreenPos = GridTopLeftCorner;
                _gridScreenPos.y = Screen.height - _gridScreenPos.y;

                var delta = invertedScreenPos - _gridScreenPos;

                var column = Mathf.FloorToInt(delta.x / (CellSize * _rectTransform.lossyScale.x));
                var row = Mathf.FloorToInt(delta.y / (CellSize * _rectTransform.lossyScale.y));

                return new Vector2(column, row);
            }

            return Vector2.negativeInfinity;
        }

        public GridCell GetGridCell(Vector2 screenPos)
        {
            var gridPos = GetGridCellRowColumn(screenPos);

            if (Single.IsNegativeInfinity(gridPos.x) || Single.IsNegativeInfinity(gridPos.y))
            {
                return null;
            }
            else
            {
                return GetGridCell((int)gridPos.x, (int)gridPos.y);
            }
        }

        public GridCell GetGridCell(int index)
        {
            if (index > 0 && index < _cells.Count)
            {
                return _cells[index];
            }

            return null;
        }

        public GridCell GetGridCell(int column, int row)
        {
            if (column >= 0 && column < NumberOfColumns && row >= 0 && row < NumberOfRows)
            {
                return _cells[GetGridCellIndex(column, row)];
            }

            return null;
        }

        public GridCell GetNeighbourGridCell(GridCell cell, Direction direction)
        {
            var directionDiff = Vector2.zero;
            switch (direction)
            {
                case Direction.Bottom:
                    directionDiff = Vector2.up; // Inverted Y-axis
                    break;
                case Direction.Top:
                    directionDiff = Vector2.down; // Inverted Y-axis
                    break;
                case Direction.Left:
                    directionDiff = Vector2.left;
                    break;
                case Direction.Right:
                    directionDiff = Vector2.right;
                    break;
            }

            var neighbourPos = cell.GridPos + directionDiff;
            return GetGridCell((int)neighbourPos.x, (int)neighbourPos.y);
        }
        #endregion

        public void SetGridMode(GridMode mode)
        {
            _currentGridMode = mode;
        }

        public void SetGridCells(Vector2 screenPos, DrawableBase drawable)
        {
            drawable.ClearCells();

            var gridPos = GetGridCellRowColumn(screenPos);
            var gridCell = _cells[GetGridCellIndex((int)gridPos.x, (int)gridPos.y)];

            if (gridCell && !gridCell.IsOccupied)
            {
                drawable.PositionOnGrid = gridPos;

                // Calculate cell positions before applying rotation
                var size = drawable.GridSize;
                var screenPositions = new List<Vector2>();
                for (var x = 0; x < size.x; ++x)
                {
                    for (var y = 0; y < size.y; ++y)
                    {
                        var posToCheck = new Vector2()
                        {
                            x = screenPos.x + (x * ScreenCellSize),
                            y = screenPos.y - (y * ScreenCellSize)
                        };

                        screenPositions.Add(posToCheck);
                    }
                }

                // Apply rotation
                var rotation = drawable.Rotation;
                var cells = new List<GridCell>();
                foreach (var point in screenPositions)
                {
                    var newCellPos = point.RotateAround(drawable.ScreenPivot, Quaternion.Euler(0, 0, rotation));
                    var cellAfterRotation = GetGridCell(newCellPos);
                    if (cellAfterRotation == null)
                    {
                        return;
                    }

                    cells.Add(cellAfterRotation);
                }

                // Link cells to drawable
                foreach (var cell in cells)
                {
                    drawable.AddCellIndex(cell.GridPos);
                }
                drawable.UpdateCells();

                ApplicationController.Instance.SelectionController.UpdateSelection();
            }
        }

        public GridData GetSaveData()
        {
            var data = new GridData()
            {
                CellSize = CellSize,
                NumberOfColumns = NumberOfColumns,
                NumberOfRows = NumberOfRows
            };

            return data;
        }

        public void LoadSaveData(GridData data)
        {
            CellSize = data.CellSize;
            NumberOfColumns = data.NumberOfColumns;
            NumberOfRows = data.NumberOfRows;

            InitializeGrid();
        }
    }
}