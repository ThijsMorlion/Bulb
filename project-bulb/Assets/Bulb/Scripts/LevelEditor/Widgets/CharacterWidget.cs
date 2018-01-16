using Bulb.Characters;
using Bulb.Controllers;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Bulb.LevelEditor.Widgets
{
    public class CharacterWidget: MonoBehaviour
    {
        public Transform SubCategoryPrefab;

        private CharacterBaseController _characterController;

        private void Awake()
        {
            _characterController = ApplicationController.Instance.CharacterController;

            // Destroy all subcategory children.
            foreach (Transform child in transform)
            {
                if(child.GetComponent<CharacterSubCategory>() != null)
                    Destroy(child.gameObject);
            }

            var characterList = _characterController.CharacterPrefabs;
            var grouping = characterList.GroupBy(c => c.Type);
            foreach (DrawableBase.DrawableType type in Enum.GetValues(typeof(DrawableBase.DrawableType)))
            {
                if (type != DrawableBase.DrawableType.None && type != DrawableBase.DrawableType.Wire)
                {
                    var subList = grouping.FirstOrDefault(g => g.Key == type);
                    AddSubCategory(type.ToString(), subList == null ? null : subList.ToList()); 
                }
            }
        }

        private void AddSubCategory(string name, List<CharacterBase> characters)
        {
            var newSubCat = Instantiate(SubCategoryPrefab, transform, false);
            var newSubCatScript = newSubCat.GetComponent<CharacterSubCategory>();

            newSubCatScript.Title.text = name;

            if(characters != null)
                characters.ForEach(newSubCatScript.AddCharacter);
        }
    }
}
