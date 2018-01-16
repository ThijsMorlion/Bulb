using System.Collections.Generic;
using Bulb.Characters;
using Bulb.Controllers;
using Bulb.Core;
using Bulb.Visuals.Grid;
using UnityEngine;

namespace Bulb.UI.Game
{
    public class ActionWidget : BasePopup
    {
        public static ActionWidget Instance;
        public DrawableBase Drawable { get; set; }

        private List<DrawableAction> _actions;

        public override void Awake()
        {
            base.Awake();

            _actions = new List<DrawableAction>();
            Instance = this;
        }

        public override void Start()
        {
            base.Start();

            BulbGrid.Instance.OnGridChanged += Instance_OnGridChanged;
        }

        private void OnDisable()
        {
            foreach (var action in _actions)
            {
                action.OnClick.RemoveAllListeners();
            }
        }

        public void RegisterAction(DrawableAction action)
        {
            if (_actions.Contains(action) == false)
            {
                action.OnClick.AddListener(() => OnActionClicked(action));
                _actions.Add(action);
            }
        }

        public override void TogglePopup(bool show)
        {
            UpdatePos();
            PlaceActions();

            base.TogglePopup(show);

        }

        private void PlaceActions()
        {
            var grid = BulbGrid.Instance;
            var gridCellSize = grid.CellSize;

            var actionsToIgnore = new List<DrawableAction>();
            if (Drawable != null)
            {
                foreach (var action in _actions)
                {
                    if ((action.Type == DrawableAction.ActionType.RotateLeft || action.Type == DrawableAction.ActionType.RotateRight) && !Drawable.CanBeRotated)
                        actionsToIgnore.Add(action);
                } 
            }

            var index = 0;
            foreach (var action in _actions)
            {
                if (!actionsToIgnore.Contains(action))
                {
                    action.gameObject.SetActive(true);

                    var rectT = action.GetComponent<RectTransform>();
                    rectT.sizeDelta = new Vector2(gridCellSize, gridCellSize);

                    var newX = Mathf.Cos(Mathf.Deg2Rad * (180 / (_actions.Count - actionsToIgnore.Count + 1)) * (index + 1)) * gridCellSize * 1.5f;
                    var newY = Mathf.Sin(Mathf.Deg2Rad * (180 / (_actions.Count - actionsToIgnore.Count + 1)) * (index + 1)) * gridCellSize * 1.5f;

                    rectT.anchoredPosition = new Vector2(newX, newY);
                    ++index; 
                }
                else
                {
                    action.gameObject.SetActive(false);
                }
            }
        }

        private void UpdatePos()
        {
            if (Drawable)
            {
                var screenPos = Drawable.ScreenRect.center;
                SetScreenPosition(screenPos);
            }
        }

        private void Instance_OnGridChanged()
        {
            UpdatePos();
            PlaceActions();
        }

        private void OnActionClicked(DrawableAction action)
        {
            var selectionController = ApplicationController.Instance.SelectionController;
            switch(action.Type)
            {
                case DrawableAction.ActionType.RotateLeft:
                    selectionController.RotateSelection(true);
                    UpdatePos();
                    break;
                case DrawableAction.ActionType.RotateRight:
                    selectionController.RotateSelection(false);
                    UpdatePos();
                    break;
                case DrawableAction.ActionType.Delete:
                    selectionController.DeleteSelectedObjects();
                    TogglePopup(false);
                    break;
            }
        }
    }
}
