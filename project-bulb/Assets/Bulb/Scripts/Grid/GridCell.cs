using Bulb.Characters;
using Bulb.Controllers;
using Settings.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.Visuals.Grid
{
    [RequireComponent(typeof(RectTransform))]
    public class GridCell : MonoBehaviour
    {
        public Image ContentImage;
        public Sprite NonEditableCellSprite;
        public TextMeshProUGUI DebugValue;
        public Vector2 GridPos { get; private set; }

        public bool IsOccupied
        {
            get
            {
                return _drawableBase != null;
            }
        }

        public Rect Rect
        {
            get
            {
                return GetComponent<RectTransform>().rect;
            }
        }

        public Vector2 CenterInScreenCoord
        {
            get
            {
                var rectTransform = GetComponent<RectTransform>();
                var screenCenterX = rectTransform.position.x + (rectTransform.sizeDelta.x / 2 * rectTransform.lossyScale.x);
                var screenCenterY = rectTransform.position.y - (rectTransform.sizeDelta.y / 2 * rectTransform.lossyScale.y);

                return new Vector2(screenCenterX, screenCenterY);
            }
        }

        private DrawableBase _drawableBase;
        public DrawableBase DrawableBase
        {
            get { return _drawableBase; }
            set
            {
                if (_drawableBase != value)
                {
                    _drawableBase = value;
                }
            }
        }

        private ApplicationController _applicationController;

        public void OnEnable()
        {
            _applicationController = ApplicationController.Instance;

            SettingsManager.Settings.ShowCellCenter.PropertyChanged += ShowCellCenter_PropertyChanged;

            ShowDebugValue(false);
        }

        public void OnDisable()
        {
            SettingsManager.Settings.ShowCellCenter.PropertyChanged -= ShowCellCenter_PropertyChanged;
        }

        public void Update()
        {
            if (ContentImage.sprite == null)
                ContentImage.color = (SettingsManager.Settings.ShowCellOccupated.Value && DrawableBase != null) == true ? Color.blue : new Color(0, 0, 0, 0);
            else
                ContentImage.color = Color.white;
        }

        public void SetDebugValue(string value)
        {
            DebugValue.text = value;
        }

        public void ShowDebugValue(bool show)
        {
            DebugValue.gameObject.SetActive(show);
        }

        private void ShowCellCenter_PropertyChanged(object sender, SettingChangedEventArgs<bool> e)
        {
            var debugController = _applicationController.DebugController;
            if (e.NewValue)
            {
                debugController.DrawDebugPoint(string.Format("{0} | Center", GetInstanceID()), CenterInScreenCoord, Color.gray);
            }
            else
            {
                debugController.DeleteDebugPoint(string.Format("{0} | Center", GetInstanceID()));
            }
        }

        public void Init(Vector2 gridPos)
        {
            GridPos = gridPos;
        }

        public void Clear()
        {
            _drawableBase = null;
            ShowDebugValue(false);
        }

        public void SetNonEditable(bool value)
        {
            if (value)
            {
                ContentImage.sprite = NonEditableCellSprite;
            }
            else
            {
                ContentImage.sprite = null;
            }
        }
    }
}