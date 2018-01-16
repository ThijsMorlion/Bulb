using UnityEngine;

public class SpawnChildPrefab : MonoBehaviour
{
    public GameObject Prefab;

    // Use this for initialization
    void Start()
    {
        if (Prefab == null)
            return;

        Instantiate(Prefab, transform, false);
    }
}