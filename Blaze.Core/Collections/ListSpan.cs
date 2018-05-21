using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Collections
{
    public class ListSpan<T> : IReadOnlyList<T>
    {
        private IReadOnlyList<T> _source;
        private int _begin;
        private int _end;
        public ListSpan(IReadOnlyList<T> source, int begin, int exclusiveEnd)
        {
            _source = source;
            if (begin > exclusiveEnd)
                throw new ArgumentException($"{nameof(begin)} > {nameof(exclusiveEnd)}");
            if (begin < 0 || begin >= source.Count)
                throw new ArgumentOutOfRangeException(nameof(begin));
            if (exclusiveEnd <= 0 || exclusiveEnd > source.Count)
                throw new ArgumentOutOfRangeException(nameof(exclusiveEnd));
            _begin = begin;
            _end = exclusiveEnd;
        }
        
        public int Count { get { return _end - _begin; } }

        public T this[int index] { get { return _source[index + _begin]; } }

        public IEnumerator<T> GetEnumerator() { return new ListSpanEnumerator(this); }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        private class ListSpanEnumerator : IEnumerator<T>
        {
            ListSpan<T> _span;
            int _ix;
            internal ListSpanEnumerator(ListSpan<T> span)
            {
                _span = span;
                _ix = -1;
            }

            public void Dispose() { }

            public bool MoveNext() { return ++_ix < _span.Count; }

            public void Reset() { _ix = -1; }

            public T Current { get { return _span[_ix]; } }

            object IEnumerator.Current { get { return Current; } }
        }
    }
}
