using Create.Persistence.Models;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Create.Persistence
{
    public abstract class PersistableBase : MonoBehaviour
    {
        protected abstract object BuildSerializationData();
        protected abstract void ApplySerializedData(object data);

        [SerializeField, HideInInspector]
        protected string _guid;
        public string GUID { get { return _guid; } }

        protected virtual void OnValidate()
        {
            if(string.IsNullOrEmpty(_guid))
            {
                _guid = Guid.NewGuid().ToString();
            }
        }

        public virtual PersistenceData Persist()
        {
            PersistenceData data = new PersistenceData() { GUID = _guid };

            //Serialize the data we want to save.
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, BuildSerializationData());

                data.Bytes = stream.ToArray();
            }

            return data;
        }

        public virtual void Restore(PersistenceData data)
        {
            if (data == null)
            {
                Debug.LogWarning(string.Format("No data was saved for object {0}.", gameObject.name));
                return;
            }

            using (MemoryStream stream = new MemoryStream(data.Bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                object persistedData = formatter.Deserialize(stream);
                ApplySerializedData(persistedData);
            }
        }
    }
}