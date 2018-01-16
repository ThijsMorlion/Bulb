using System;
using UnityEngine;

namespace Create.Persistence.Models
{
    [Serializable]
    public class TransformData
    {
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] LocalScale { get; set; }

        public TransformData()
        {
            Position = new float[3];
            Rotation = new float[4];
            LocalScale = new float[3];
        }

        public TransformData(Transform transform)
        {
            Position = new float[] { transform.position.x, transform.position.y, transform.position.z };
            Rotation = new float[] { transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w };
            LocalScale = new float[] { transform.localScale.x, transform.localScale.y, transform.localScale.z };
        }

        public void FillTransform(Transform target)
        {
            try
            {
                target.position = new Vector3(Position[0], Position[1], Position[2]);
                target.rotation = new Quaternion(Rotation[0], Rotation[1], Rotation[2], Rotation[3]);
                target.localScale = new Vector3(LocalScale[0], LocalScale[1], LocalScale[2]);
            }
            catch(ArgumentOutOfRangeException)
            {
                Debug.LogWarningFormat("[{0}] Persisted transform is invalid - not all array items are initialized.");
            }
        }
    }
}