using Bulb.Data;
using UnityEngine;

namespace Bulb.Characters
{
    public class SwitchCharacter : CharacterBase
    {
        [Header("Switch Properties")]
        public Texture2D ClosedTexture;
        public Texture2D OpenTexture;

        private bool _isOpen = true;
        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if(_isOpen != value)
                {
                    _isOpen = value;

                    var texture = _isOpen == true ? OpenTexture : ClosedTexture;
                    SetImage(texture, Color.white);
                }
            }
        }

        public new SwitchCharacterData GetSaveData()
        {
            var charData = base.GetSaveData();
            var data = new SwitchCharacterData()
            {
                GridCellPositions = charData.GridCellPositions,
                PositionOnGrid = charData.PositionOnGrid,
                Rotation = charData.Rotation,
                State = charData.State,
                Type = charData.Type
            };

            return data;
        }

        public override void Reset()
        {
            base.Reset();

            IsOpen = true;
        }

        public void ToggleSwitch()
        {
            IsOpen = !IsOpen;
        }
    }
}