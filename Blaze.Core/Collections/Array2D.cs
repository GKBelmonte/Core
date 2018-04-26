using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Collections
{
    public class Array2D<T>
    {
        /*
         *  Jagged arrays out perform multi-dimensional arrays in C# despite that 
         *   multi-dimensional arrays 
         */
        private int _rowCount;
        private int _columnCount;
        private T[] _vals;

        public Array2D(int rowCount, int columnCount)
        {
            _rowCount = rowCount;
            _columnCount = columnCount;
            _vals = new T[_rowCount * _columnCount];
            var rows = new List<T>(_rowCount);
            Rows = Enumerable.Range(0, _rowCount).Select(r => new Row(this, r)).ToList();
            Columns = Enumerable.Range(0, _columnCount).Select(c => new Column(this, c)).ToList();
        }

        public int RowCount { get { return _rowCount; } }
        public int ColumnCount { get { return _columnCount; } }

        public int RowSize { get { return _columnCount; } }
        public int ColumnSize { get { return _rowCount; } }
        
        public T this[int r, int c]
        {
            get
            {
                if (r > _rowCount)
                    throw new ArgumentOutOfRangeException(nameof(r));
                if (c > _columnCount)
                    throw new ArgumentOutOfRangeException(nameof(c));
                return _vals[r * _rowCount + c];
            }
            set
            {
                if (r > _rowCount)
                    throw new ArgumentOutOfRangeException(nameof(r));
                if (c > _columnCount)
                    throw new ArgumentOutOfRangeException(nameof(c));
                _vals[r * _rowCount + c] = value;
            }
        }

        public IReadOnlyList<IReadOnlyList<T>> Rows { get; }
        public IReadOnlyList<IReadOnlyList<T>> Columns { get; }

        private class Row : IReadOnlyList<T>
        {
            private readonly Array2D<T> _parent;
            private readonly int _rowNumber;
            public Row(Array2D<T> parent, int r)
            {
                _parent = parent;
                _rowNumber = r;
            }

            public T this[int index] { get { return _parent[_rowNumber, index]; }  }

            public int Count { get { return _parent.RowSize; } }

            public IEnumerator<T> GetEnumerator() { return new ListEnumerator(this); }

            IEnumerator IEnumerable.GetEnumerator() { return new ListEnumerator(this); }
        }

        private class Column : IReadOnlyList<T>
        {
            private readonly Array2D<T> _parent;
            private readonly int _columnNumber;
            public Column(Array2D<T> parent, int c)
            {
                _parent = parent;
                _columnNumber = c;
            }

            public T this[int index] { get { return _parent[index, _columnNumber]; } }

            public int Count { get { return _parent.ColumnSize; } }

            public IEnumerator<T> GetEnumerator() { return new ListEnumerator(this); }

            IEnumerator IEnumerable.GetEnumerator() { return new ListEnumerator(this); }
        }

        private class ListEnumerator : IEnumerator<T>, IEnumerator
        {
            private int _CurrentIx = 0;
            private readonly IReadOnlyList<T> _parent;

            public ListEnumerator(IReadOnlyList<T> parent) { _parent = parent; }

            public object Current => _parent[_CurrentIx];

            T IEnumerator<T>.Current => _parent[_CurrentIx];

            public bool MoveNext() { return ++_CurrentIx != _parent.Count; }

            public void Reset() { _CurrentIx = 0; }

            public void Dispose() { }
        }
    }
}
