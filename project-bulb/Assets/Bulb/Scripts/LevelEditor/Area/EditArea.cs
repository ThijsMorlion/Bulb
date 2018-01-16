using Bulb.Controllers;
using Bulb.Core;
using Bulb.Visuals.Grid;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using TMPro;
using Bulb.Game;

namespace Bulb.LevelEditor.Area
{
    public class EditArea : MonoBehaviour
    {
        public TextMeshProUGUI LevelName;
        public TextMeshProUGUI BuildVersionNumber;

        private TapGesture _tapGesture;
        private ScreenTransformGesture _transformGesture;

        private BulbGrid _grid;
        private SelectionController _selectionController;

        private float _baseZoomValue = 1f;
        private float _zoomValue = 1f;
        private float _scrollFactor = 0.05f;

        private void Awake()
        {
            _grid = FindObjectOfType<BulbGrid>();
            _selectionController = ApplicationController.Instance.SelectionController;

            _tapGesture = transform.GetOrAddComponent<TapGesture>();
            _tapGesture.Tapped += _tapGesture_Tapped;

            _transformGesture = transform.GetOrAddComponent<ScreenTransformGesture>();
            _transformGesture.TransformStarted += _transformGesture_TransformStarted;
            _transformGesture.Transformed += _transformGesture_Transformed;
            _transformGesture.TransformCompleted += _transformGesture_TransformCompleted;

            LevelController.OnLevelLoaded += (levelName, levelData) => LevelName.text = levelName;
            DataController.OnLevelSaved += (levelName) => LevelName.text = levelName;

            BuildVersionNumber.text = Application.version;

            GameState.CurrentState = GameStates.Editor;
        }

        private void _transformGesture_TransformStarted(object sender, System.EventArgs e)
        {
            _grid.OnDragStarted(sender, e);
        }

        private void _transformGesture_Transformed(object sender, System.EventArgs e)
        {
            _grid.OnDragUpdate(sender, e);
        }

        private void _transformGesture_TransformCompleted(object sender, System.EventArgs e)
        {
            _grid.OnDragCompleted(sender, e);
        }

        private void _tapGesture_Tapped(object sender, System.EventArgs e)
        {
            _grid.OnTapped(sender, e);
        }

        private void Update()
        {
            if(Input.mouseScrollDelta.SqrMagnitude() != 0)
            {
                _selectionController.ClearSelection();
                _zoomValue = _baseZoomValue + Input.mouseScrollDelta.y * _scrollFactor;

                if (_zoomValue > 0)
                    _grid.ZoomGrid(_zoomValue);
            }
        }

        public void Reset()
        {
            LevelName.text = "";
        }
    }
}