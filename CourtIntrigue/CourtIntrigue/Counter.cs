using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtIntrigue
{
    class Counter<T> : IEnumerable<KeyValuePair<T, int>>
    {
        private Dictionary<T, int> counts = new Dictionary<T, int>();

        public void Increment(T which)
        {
            if (counts.ContainsKey(which))
                ++counts[which];
            else
                counts.Add(which, 1);
        }

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<T, int>>)counts).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<T, int>>)counts).GetEnumerator();
        }
    }
}
