using UnityEngine;

namespace Bulb.Electricity
{
    public enum Pole
    {
        Positive,
        Negative,
        None
    }

    public class ConnectionPole : MonoBehaviour
    {
        public Pole Pole;
    }
}