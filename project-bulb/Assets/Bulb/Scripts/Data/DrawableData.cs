using Bulb.Characters;
using System.Collections.Generic;
using UnityEngine;

namespace Bulb.Data
{
    public class DrawableData
    {
        public DrawableBase.DrawableType Type;
        public SerializableVector2 PositionOnGrid;
        public HashSet<SerializableVector2> GridCellPositions;
        public float Rotation;
    }
}