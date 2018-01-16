using System.Linq;
using Bulb.Characters;
using Bulb.Controllers;
using Bulb.Core;
using UnityEngine;

namespace Bulb.UI.Game
{
    public class Inventory : MonoBehaviour
    {
        public InventoryCharacterItem InventoryItemPrefab;

        public void OnEnable()
        {
            LevelController.OnLevelLoaded += DataController_OnLevelLoaded;
        }

        public void OnDisable()
        {
            LevelController.OnLevelLoaded -= DataController_OnLevelLoaded;
        }

        private void DataController_OnLevelLoaded(string levelName, Data.LevelData data)
        {
            var characterController = ApplicationController.Instance.CharacterController;
            if (data.Max4VBatteries > 0)
            {
                var batteries = characterController.CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Battery)
                                                              .OrderBy(b => ((BatteryCharacter)b).Battery.Voltage)
                                                              .ToList();
                InstantiateInventoryItem(batteries[0], data.Max4VBatteries);
            }

            if (data.Max9VBatteries > 0)
            {
                var batteries = characterController.CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Battery)
                                                              .OrderBy(b => ((BatteryCharacter)b).Battery.Voltage)
                                                              .ToList();
                InstantiateInventoryItem(batteries[1], data.Max9VBatteries);
            }

            if (data.MaxBulbs > 0)
            {
                var bulb = characterController.CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Bulb).SingleOrDefault();
                InstantiateInventoryItem(bulb, data.MaxBulbs);
            }

            if(data.MaxBuzzers > 0)
            {
                var buzzer = characterController.CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Buzzer).SingleOrDefault();
                InstantiateInventoryItem(buzzer, data.MaxBuzzers);
            }

            if (data.MaxMotors > 0)
            {
                var motor = characterController.CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Motor).SingleOrDefault();
                InstantiateInventoryItem(motor, data.MaxMotors);
            }

            if (data.MaxSwitches > 0)
            {
                var switchPrefab = characterController.CharacterPrefabs.Where(c => c.Type == DrawableBase.DrawableType.Switch).SingleOrDefault();
                InstantiateInventoryItem(switchPrefab, data.MaxSwitches);
            }
        }

        private void InstantiateInventoryItem(CharacterBase character, int count)
        {
            var inventoryItem = Instantiate(InventoryItemPrefab, transform, false);
            inventoryItem.CharacterPrefab = character;
            //inventoryItem.Icon.overrideSprite = Sprite.Create(character.Icon, new Rect(0, 0, character.Icon.width, character.Icon.height), Vector2.one / 2);

            var currentlevel = ApplicationController.Instance.LevelController.CurrentLevel;
            switch (character.Type)
            {
                case DrawableBase.DrawableType.Battery:
                    var battery = (BatteryCharacter)character;
                    if (battery.Battery.Voltage == 4)
                    {
                        inventoryItem.SetType(Level.PropertyType.Battery4V);
                        currentlevel.Max4VBatteries = count;
                    }
                    else
                    {
                        inventoryItem.SetType(Level.PropertyType.Battery9V);
                        currentlevel.Max9VBatteries = count;
                    }
                    break;
                case DrawableBase.DrawableType.Bulb:
                    inventoryItem.SetType(Level.PropertyType.Bulb);
                    currentlevel.MaxBulbs = count;
                    break;
                case DrawableBase.DrawableType.Motor:
                    inventoryItem.SetType(Level.PropertyType.Motor);
                    currentlevel.MaxMotors = count;
                    break;
                case DrawableBase.DrawableType.Buzzer:
                    inventoryItem.SetType(Level.PropertyType.Buzzer);
                    currentlevel.MaxBuzzers = count;
                    break;
                case DrawableBase.DrawableType.Switch:
                    inventoryItem.SetType(Level.PropertyType.Switch);
                    currentlevel.MaxSwitches = count;
                    break;
            }
        }
    }
}
