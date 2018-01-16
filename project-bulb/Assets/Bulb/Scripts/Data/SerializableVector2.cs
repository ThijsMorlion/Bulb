using UnityEngine;

namespace Bulb.Data
{
    public class SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 GetVector2()
        {
            return new Vector2(x, y);
        }
    }
}