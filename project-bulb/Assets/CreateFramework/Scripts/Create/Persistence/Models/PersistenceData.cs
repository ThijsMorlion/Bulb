using System;

namespace Create.Persistence.Models
{
    [Serializable]
    public class PersistenceData
    {
        public string GUID { get; set; }
        public byte[] Bytes { get; set; }
    }
}