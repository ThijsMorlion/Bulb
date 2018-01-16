using System;
using System.Collections.Generic;

namespace Splines.Bezier
{
    /// <summary>
    /// Unity cannot serialize nested lists. Generic wrapper classes cannot be serialized either.
    /// </summary>
    [Serializable]
    public class ListWrapper
    {
        public List<int> List = new List<int>();
    }
}
