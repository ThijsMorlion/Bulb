using Bulb.Controllers;
using Bulb.Core;
using Bulb.Data;
using Bulb.Visuals.Grid;
using System;
using System.Collections.Generic;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.Characters
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(TapGesture))]
    public class DrawableBase : MonoBehaviour
    {
        public enum DrawableType
        {
            None,
            Bulb,
            Battery,
            Obstruction,
            Switch,
            Motor,
            Wire,
            Buzzer
        }

        [Header("Drawable Properties")]
        public DrawableType Type;
        public Vector2 GridSize;
        public bool CanBeRotated;
        public bool CanBeMoved;
        public bool CanBeSelected;

        private Image _image;
        public Image Image
        {
            get { return _image; }
        }

        public Color DebugColor { get; set; }

        private Color _overrideColor = Color.white;
        public Color OverrideColor
        {
            get { return _overrideColor; }
            set
            {
                if(_overrideColor != value)
                {
                    _overrideColor = value;
                    _image.color = _overrideColor;
                }
            }
        }

        public Vector2 PositionOnGrid { get; set; }
        public List<GridCell> Cells
        {
            get
            {
                var cells = new List<GridCell>();
                foreach(var pos in _gridCellPositions)
                {
                    cells.Add(_grid.GetGridCell((int)pos.x, (int)pos.y));
                }

                return cells;
            }
        }

        protected HashSet<Vector2> _gridCellPositions = new HashSet<Vector2>();
        public void AddCellIndex(Vector2 gridPos)
        {
            _gridCellPositions.Add(gridPos);
        }

        public void UpdateCells()
        {
            Cells.ForEach(c =>
            {
                c.DrawableBase = this;
            });
        }

        public Vector2 LocalPivot
        {
            get
            {
                return _image.GetComponent<RectTransform>().pivot;
            }
        }

        public Vector2 ScreenPivot
        {
            get
            {
                var rectT = _image.GetComponent<RectTransform>();
                var screenRotationPointX = rectT.position.x + (rectT.pivot.x * rectT.sizeDelta.x * rectT.lossyScale.x);
                var screenRotationPointY = rectT.position.y - (rectT.pivot.y * rectT.sizeDelta.y * rectT.lossyScale.y);

                return new Vector2(screenRotationPointX, screenRotationPointY);
            }
        }

        public Rect ScreenRect
        {
            get
            {
                var rectT = GetComponent<RectTransform>();
                var rect = new Rect
                {
                    xMin = rectT.position.x - (_grid.CellSize / 2 * rectT.lossyScale.x),
                    yMin = rectT.position.y + (_grid.CellSize / 2 * rectT.lossyScale.y)
                };

                rect.xMax = rect.xMin + (rectT.sizeDelta.x * rectT.lossyScale.x);
                rect.yMax = rect.yMin - (rectT.sizeDelta.y * rectT.lossyScale.y); 

                if (Rotation != 0)
                    return rect.RotateRectAroundPivot(ScreenPivot, Quaternion.Euler(0, 0, Rotation));

                return rect;
            }
        }

        public float Rotation { get; protected set; }

        protected BulbGrid _grid;

        public void Destroy()
        {
            // Clear cells the drawable occupies
            ClearCells();

            // Destroy itself
            Destroy(gameObject);
        }

        public void SetImage(Texture2D source, Color color)
        {
            _image.color = color;
            _image.sprite = null;
            _image.sprite = Sprite.Create(source, new Rect(0, 0, source.width, source.height), Vector2.zero);
        }

        public void ClearCells()
        {
            foreach (var cell in Cells)
            {
                cell.Clear();
            }

            _gridCellPositions.Clear();
        }

        public void RotateAroundPivot(float angle)
        {
            Rotation += angle;
            if (Rotation > 360)
                Rotation -= 360f;

            if (Rotation < -360)
                Rotation += 360f;

            SetRotationAroundPivot(Rotation);
        }

        public void SetRotationAroundPivot(float angle)
        {
            Rotation = angle;
            _image.transform.rotation = Quaternion.Euler(0, 0, Rotation);
        }

        protected virtual void Awake()
        {
            _grid = FindObjectOfType<BulbGrid>();
            _image = GetComponentInChildren<Image>();

            DebugColor = Color.white;
        }

        protected virtual void Update()
        {
            var debugController = ApplicationController.Instance.DebugController;
            if (SettingsManager.Settings.ShowDrawablePivot.Value)
            {
                debugController.DrawDebugPoint(string.Format("{0} | Pivot", GetInstanceID()), ScreenPivot, Color.magenta);
            }
            else
            {
                debugController.DeleteDebugPoint(string.Format("{0} | Pivot", GetInstanceID()));
            }

            if (SettingsManager.Settings.ShowDebugColors.Value)
                _image.color = DebugColor;
            else
                _image.color = _overrideColor;
        }

        public DrawableData GetSaveData()
        {
            var newData = new DrawableData()
            {
                GridCellPositions = new HashSet<SerializableVector2>(),
                PositionOnGrid = new SerializableVector2(PositionOnGrid.x, PositionOnGrid.y),
                Rotation = Rotation,
                Type = Type
            };

            foreach(var gridCellPos in _gridCellPositions)
            {
                newData.GridCellPositions.Add(new SerializableVector2(gridCellPos.x, gridCellPos.y));
            }

            return newData;
        }

        public override string ToString()
        {
            return string.Format("Type: {0} | Position: {1}", Type, PositionOnGrid);
        }
    }
}
