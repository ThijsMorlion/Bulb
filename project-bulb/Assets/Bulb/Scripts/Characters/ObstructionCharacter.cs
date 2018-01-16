using Bulb.Data;

namespace Bulb.Characters
{
    public class ObstructionCharacter : CharacterBase
    {
        public new ObstructionCharacterData GetSaveData()
        {
            var charData = base.GetSaveData();
            var data = new ObstructionCharacterData()
            {
                GridCellPositions = charData.GridCellPositions,
                PositionOnGrid = charData.PositionOnGrid,
                Rotation = charData.Rotation,
                State = charData.State,
                Type = charData.Type
            };

            return data;
        }
    }
}