using Bulb.Characters;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Bulb.LevelEditor.Tools;
using Bulb.Characters.Wire;
using TouchScript.Gestures.TransformGestures;
using Bulb.Game;

namespace Bulb.Controllers
{
    public class SelectionController : MonoBehaviour
    {
        public RectTransform SelectionPrefab;
        public RectTransform SelectionAreaPrefab;

        public bool DebugMode;
        public RectTransform MouseDebugCircle;

        private HashSet<DrawableBase> _selectedObjects = new HashSet<DrawableBase>();

        private List<Transform> _selectionObjects = new List<Transform>();

        public delegate void ObjectDeleted(DrawableBase drawable);
        public static event ObjectDeleted OnObjectDeleted;

        #region DRAG PROPERTIES
        public delegate void DragCompleted(RectTransform rect);
        public DragCompleted OnSelectionAreaDrawCompleted;

        private RectTransform _selectionAreaObject;
        private Rect _dragRect;
        private Vector2 _startDragPosition;

        private RectTransform _debugStart;
        private RectTransform _debugEnd;
        private bool _isSelecting = false;

        private RectTransform _rectVisual;
        #endregion

        public bool IsPartOfSelection(DrawableBase drawable)
        {
            return _selectedObjects.Contains(drawable);
        }

        public void AddToSelection(DrawableBase drawable)
        {
            if (_selectedObjects.Add(drawable))
                UpdateSelection();
        }

        public void RemovefromSelection(DrawableBase drawable)
        {
            _selectedObjects.Remove(drawable);
            UpdateSelection();
        }

        public void RotateSelection(bool rotateLeft)
        {
            _selectedObjects.ToList().ForEach(o =>
               {
                   if (o.CanBeRotated)
                   {
                       if (rotateLeft)
                       {
                           RotateTool.RotateLeft(o);
                       }
                       else
                       {
                           RotateTool.RotateRight(o);
                       } 
                   }
               });
        }

        public void ClearSelection()
        {
            _selectedObjects.Clear();

            ClearSelectionObjects();
            UpdateSelection();
        }

        private void ClearSelectionObjects()
        {
            foreach (var obj in _selectionObjects)
            {
                Destroy(obj.gameObject);
            }

            _selectionObjects.Clear();
        }

        public void StartMove(DrawableBase moveHandle, ScreenTransformGesture gesture)
        {
            _startDragPosition = gesture.ScreenPosition;
            MoveTool.StartMove(moveHandle, _startDragPosition, _selectedObjects.ToList());
        }

        public void UpdateMove(ScreenTransformGesture gesture)
        {
            var debugController = ApplicationController.Instance.DebugController;
            debugController.DrawDebugPoint(string.Format("{0} ScreenPos", GetInstanceID()), gesture.ScreenPosition, Color.cyan);

            MoveTool.UpdateMove(gesture.ScreenPosition - _startDragPosition);
        }

        public void StopMove(ScreenTransformGesture gesture)
        {
            MoveTool.StopMove(gesture.ScreenPosition - _startDragPosition);

            var debugController = ApplicationController.Instance.DebugController;
            debugController.DeleteDebugPoint(string.Format("{0} ScreenPos", GetInstanceID()));
        }

        #region SELECT_AREA
        public void StartDrag(Vector2 pos)
        {
            ClearSelection();

            var overlayCanvas = ApplicationController.Instance.CanvasController.SelectionContainer;
            _selectionAreaObject = Instantiate(SelectionAreaPrefab, overlayCanvas, false);
            _selectionAreaObject.position = pos;
            _selectionAreaObject.sizeDelta = Vector2.zero;
            _selectionAreaObject.pivot = new Vector2(0, 1);

            _startDragPosition = pos;

            if (DebugMode)
            {
                _debugStart = Instantiate(MouseDebugCircle, overlayCanvas, false);
                _debugStart.position = pos;
            }

            _isSelecting = true;
        }

        public void Drag(Vector2 currMousePos)
        {
            if (_isSelecting)
            {
                var overlayCanvas = ApplicationController.Instance.CanvasController.MainCanvas;

                _dragRect = new Rect()
                {
                    x = Mathf.Min(_startDragPosition.x, currMousePos.x),
                    y = Mathf.Max(_startDragPosition.y, currMousePos.y),
                    width = overlayCanvas.sizeDelta.x * (Mathf.Abs(_startDragPosition.x - currMousePos.x) / Screen.width),
                    height = overlayCanvas.sizeDelta.y * (Mathf.Abs(_startDragPosition.y - currMousePos.y) / Screen.height)
                };

                if (DebugMode)
                {
                    if (_debugEnd == null)
                        _debugEnd = Instantiate(MouseDebugCircle, overlayCanvas, false);
                    _debugEnd.position = currMousePos;

                    if (_rectVisual == null)
                        _rectVisual = Instantiate(MouseDebugCircle, overlayCanvas, false);
                    _rectVisual.position = new Vector2(_dragRect.x, _dragRect.y);
                }

                _selectionAreaObject.position = new Vector2(_dragRect.x, _dragRect.y);
                _selectionAreaObject.sizeDelta = new Vector2(_dragRect.width, _dragRect.height); 
            }
        }

        public void EndDrag()
        {
            if (_isSelecting)
            {
                if (DebugMode)
                {
                    Destroy(_debugStart.gameObject);
                    Destroy(_debugEnd.gameObject);
                    Destroy(_rectVisual.gameObject);

                    _debugStart = null;
                    _debugEnd = null;
                    _rectVisual = null;
                }

                if (OnSelectionAreaDrawCompleted != null)
                    OnSelectionAreaDrawCompleted(_selectionAreaObject);

                Destroy(_selectionAreaObject.gameObject);
                _selectionAreaObject = null;
                _isSelecting = false;
            }
        }
        #endregion

        public void UpdateSelection()
        {
            if (GameState.CurrentState == GameStates.Editor)
            {
                var overlayCanvas = ApplicationController.Instance.CanvasController.SelectionContainer;
                foreach (var obj in _selectedObjects)
                {
                    var newSelectionObject = Instantiate(SelectionPrefab, overlayCanvas, false);
                    var imageRectT = obj.Image.GetComponent<RectTransform>();
                    newSelectionObject.position = imageRectT.position;
                    newSelectionObject.sizeDelta = imageRectT.rect.size;
                    newSelectionObject.rotation = imageRectT.rotation;
                    newSelectionObject.pivot = imageRectT.pivot;

                    _selectionObjects.Add(newSelectionObject);
                } 
            }
        }

        public void DeleteSelectedObjects()
        {
            foreach (var selectedObject in _selectedObjects)
            {
                if (OnObjectDeleted != null)
                    OnObjectDeleted(selectedObject);

                if (selectedObject.Type == DrawableBase.DrawableType.Wire)
                {
                    var wirePiece = (WirePiece)selectedObject;
                    var wireController = ApplicationController.Instance.WireController;
                    wireController.DeleteWirePiece(wirePiece);
                }
                else
                {
                    var character = (CharacterBase)selectedObject;
                    var charController = ApplicationController.Instance.CharacterController;
                    charController.DeleteCharacter(character);
                }
            }

            _selectedObjects.Clear();

            ClearSelection();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Backspace))
                DeleteSelectedObjects();
        }
    }
}