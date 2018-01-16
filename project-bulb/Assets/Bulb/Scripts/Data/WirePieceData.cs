using Bulb.Visuals.Grid;
using Bulb.Characters.Wire;
using System.Collections.Generic;

namespace Bulb.Data
{
    public class WirePieceData
    {
        public bool IsBridge;
        public WirePiece.VisualType Type;
        public Direction DirectionFlag;
        public Dictionary<Direction, SerializableVector2> Connections;
        public DrawableData DrawableData;
    }
}