using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public class Map<T1, T2>
    {
        public Map()
        {
            Forward = new Dictionary<T1, T2>();
            Reverse = new Dictionary<T2, T1>();
        }

        public void Add(T1 t1, T2 t2)
        {
            Forward.Add(t1, t2);
            Reverse.Add(t2, t1);
        }

        public Dictionary<T1, T2> Forward { get; private set; }
        public Dictionary<T2, T1> Reverse { get; private set; }

        public int Count { get { return Forward.Count; }}

        public string ToDebugString()
        {
            return string.Join(
                ",", 
                Forward.Select(kvp => $"({kvp.Key}, {kvp.Value})"));
        }
    }
}
