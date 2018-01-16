using UnityEngine;

namespace Bulb.Core
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private static GameObject _instance;

        void Awake()
        {
            if (_instance != null)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                _instance = gameObject;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}