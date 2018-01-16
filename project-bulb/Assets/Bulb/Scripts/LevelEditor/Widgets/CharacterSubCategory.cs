using Bulb.Characters;
using TMPro;
using UnityEngine;

namespace Bulb.LevelEditor.Widgets
{
    public class CharacterSubCategory : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public CharacterWidgetItem CharacterWidgetItem;
        public Transform CharacterContainer;

        public void AddCharacter(CharacterBase character)
        {
            var imageInstance = Instantiate(CharacterWidgetItem, CharacterContainer, false);
            imageInstance.CharacterPrefab = character;
            imageInstance.SetImage(character.Icon);
        }
    }
}