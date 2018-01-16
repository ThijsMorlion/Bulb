using Bulb.Controllers;
using Bulb.Core;
using Bulb.Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Popups
{
    public class CreateBridgePopup : BasePopup
    {
        public static CreateBridgePopup Instance;
        public OnCreateBridgeConfirmedEvent OnCreateBridgeConfirmed;
        public Transform ButtonGroup;
        public Button BridgeButton;
        public Button SnapButton;

        public override void Awake()
        {
            base.Awake();

            OnCreateBridgeConfirmed = new OnCreateBridgeConfirmedEvent();
            Instance = this;
        }

        public void CreateBridge(bool value)
        {
            base.TogglePopup(false);

            if (OnCreateBridgeConfirmed != null)
            {
                OnCreateBridgeConfirmed.Invoke(value);
            }
        }

        public override void TogglePopup(bool show)
        {
            if (GameState.CurrentState == GameStates.Game)
            {
                var currentLevel = ApplicationController.Instance.LevelController.CurrentLevel;

                BridgeButton.gameObject.SetActive(currentLevel.CanBridge);
                SnapButton.gameObject.SetActive(currentLevel.CanSnap);

                if (currentLevel.CanBridge || currentLevel.CanSnap)
                    base.TogglePopup(show); 
            }
            else
            {
                base.TogglePopup(show);
            }
        }

        public override void SetScreenPosition(Vector2 pos)
        {
            ButtonGroup.position = pos;
        }
    }

    public class OnCreateBridgeConfirmedEvent : UnityEvent<bool>
    {

    }
}