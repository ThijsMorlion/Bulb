using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System;
using Create.Persistence.Models;

namespace Create.Persistence
{
    /// <summary>
    /// Handles the saving and loading of all data.
    /// </summary>
    public class PersistenceManager : MonoBehaviour
    {
        public virtual void Save(string path)
        {
            //Get all persistable objects, and have them serialize their properties.
            PersistableBase[] persistables = FindObjectsOfType<PersistableBase>();
            List<PersistenceData> data = new List<PersistenceData>();
            foreach (var persistable in persistables)
            {
                data.Add(persistable.Persist());
            }

            var fileInfo = new FileInfo(path);
            Directory.CreateDirectory(fileInfo.DirectoryName);

            //Serialize the list of data and write it to the save file.
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);

                try
                {
                    File.WriteAllBytes(path, stream.ToArray());
                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("[{0}] {1} Failed to save file \"{2}\": {3}\r\n{4}", GetType(), DateTime.Now, path, ex.Message, ex.StackTrace);
                }
            }
        }

        public virtual void Load(string path)
        {
            List<PersistenceData> data = null;

            //Deserialize the list of data.
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data = (List<PersistenceData>)formatter.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[{0}] {1} Failed to load file \"{2}\": {3}\r\n{4}", GetType(), DateTime.Now, path, ex.Message, ex.StackTrace);
            }

            //Get all persistable objects, and have them load their respective properties.
            PersistableBase[] persistables = FindObjectsOfType<PersistableBase>();
            foreach (var persistable in persistables)
            {
                persistable.Restore(data.FirstOrDefault(d => d.GUID == persistable.GUID));
            }
        }
    }
}