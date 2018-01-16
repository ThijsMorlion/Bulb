using System;
using Bulb.Characters;
using Bulb.Controllers;
using Bulb.Core;
using Bulb.Core.Bindables;
using Bulb.LevelEditor.Widgets;
using TMPro;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

namespace Bulb.UI.Game
{
    [RequireComponent(typeof(TransformGesture))]
    public class InventoryCharacterItem : CharacterWidgetItem
    {
        public TextMeshProUGUI AmountText;

        private int _currentAmount
        {
            get
            {
                return int.Parse(AmountText.text);
            }
        }

        public void SetType(Level.PropertyType type)
        {
            var bindableBase = AmountText.GetComponent<BindLevelInt>();
            bindableBase.Type = type;
        }

        protected override void _transformGesture_TransformStarted(object sender, EventArgs e)
        {
            if (_currentAmount > 0)
            {
                base._transformGesture_TransformStarted(sender, e);
            }
            else
            {
                _transformGesture.Cancel();
            }
        }

        protected override void _transformGesture_TransformCompleted(object sender, EventArgs e)
        {
            base._transformGesture_TransformCompleted(sender, e);

            if (_canBePlaced)
            {
                var currLevel = ApplicationController.Instance.LevelController.CurrentLevel;
                switch (CharacterPrefab.Type)
                {
                    case DrawableBase.DrawableType.Battery:
                        var battery = (BatteryCharacter)CharacterPrefab;
                        if (battery.Battery.Voltage == 4)
                            --currLevel.Max4VBatteries;
                        else
                            --currLevel.Max9VBatteries;
                        break;
                    case DrawableBase.DrawableType.Bulb:
                        --currLevel.MaxBulbs;
                        break;
                    case DrawableBase.DrawableType.Buzzer:
                        --currLevel.MaxBuzzers;
                        break;
                    case DrawableBase.DrawableType.Motor:
                        --currLevel.MaxMotors;
                        break;
                    case DrawableBase.DrawableType.Switch:
                        --currLevel.MaxSwitches;
                        break;
                }
            }
        }
    }
}
