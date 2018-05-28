using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;
using Blaze.Core.Collections;

namespace Blaze.Cryptography.Classics
{
    /// <summary>
    /// Simple classic transposition.
    /// Like the transpose of a Matrix
    /// </summary>
    public class TranspositionCypher : AlphabeticCypher
    {
        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnCount = GetColumnCount(plain.Length, rng);

            byte[] cypher = Encrypt(plain, columnCount);

            return cypher;
        }

        public virtual byte[] Encrypt(byte[] plain, int columnCount)
        {
            int[] plainIx = ByteToIndices(plain);

            int[] cypher = Encrypt(plainIx, columnCount);

            return IndicesToBytes(cypher);
        }

        protected int[] Encrypt(IReadOnlyList<int> plainIx, int columnCount)
        {
            int rowSize = columnCount;
            
            List<int> cypher = new List<int>(plainIx.Count);
            for (int i = 0; i < rowSize; ++i)
            {
                for (int j = i; j < plainIx.Count; j += rowSize)
                    cypher.Add(plainIx[j]);
            }
            return cypher.ToArray();
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnCount = GetColumnCount(cypher.Length, rng);

            byte[] plain = Decrypt(cypher, columnCount);

            return plain;
        }

        public virtual byte[] Decrypt(byte[] cypher, int columnCount)
        {
            int[] cypherIx = ByteToIndices(cypher);

            int[] plainIx = Decrypt(cypherIx, columnCount);

            return IndicesToBytes(plainIx);
        }

        protected int[] Decrypt(IReadOnlyList<int> cypherIx, int originalColumnCount)
        {
            // new columnSize is old columnCount
            int columnSize = originalColumnCount;
            int rowCount = columnSize;
            int remainder = cypherIx.Count % originalColumnCount;
            //remainder == originally complete Columns / complete Rows
            int completeRows = remainder == 0 ? rowCount : remainder;
            
            //new rowSize is length / original column number
            // rowSize is number of columns
            int rowSize = cypherIx.Count / originalColumnCount + (remainder == 0 ? 0 : 1);
            int columnCount = rowSize;

            List<int> plainIx = new List<int>(cypherIx.Count);
            for (int i = 0; i < columnCount; ++i)
            {
                int currentRowCount = 0;
                int j = i;
                //Get the indexes (j) where the rows are complete (+=rowSize)
                for (; currentRowCount < completeRows && j < cypherIx.Count; ++currentRowCount)
                {
                    plainIx.Add(cypherIx[j]);
                    j += rowSize;
                }

                //For the last column, we only want complete rows
                if (i == columnCount - 1)
                    continue;

                //Get the indexes (j) where the rows are incomplete (+=rowSize -1 )
                for(; currentRowCount < rowCount && j < cypherIx.Count; ++currentRowCount)
                {
                    plainIx.Add(cypherIx[j]);
                    j += rowSize - 1;
                }
            }

            return plainIx.ToArray();
        }

        protected static int GetColumnCount(int plainLength, IRng rng)
        {
            //ranges computed as +/- 25% from the average between sqrt(min) and sqrt(max)
            //(except for tiny values)
            // i.e. range +/-25% (sqrt(min)+sqrt(max))/2
            int next = rng.Next();

            return GetColCount(plainLength, next);
        }

        protected static int GetReverseColumnCount(int cypherLength, IRng rng)
        {
            int next = rng.Next();

            return GetColCount(cypherLength, next);
        }

        private static int GetColCount(int plainLength, int next)
        {
            int columnCount = 0;
            if (plainLength <= 16) //4x4
                columnCount = ClampNext(next, 3, 6);
            else if (plainLength <= 64)  //8*8
                columnCount = ClampNext(next, 4, 8);
            else if (plainLength <= 256)  //16*16
                columnCount = ClampNext(next, 9, 15);
            else if (plainLength <= 1024)  //32*32
                columnCount = ClampNext(next, 18, 30);
            else if (plainLength <= 4096) //64*64
                columnCount = ClampNext(next, 36, 60);
            return columnCount;
        }

        private static int GetColCountV2(int plainLength, int next)
        {
            //+/-1/3*ave(sqrt(min),sqrt(max))
            int root = next;
            int columnCount = 0;
            if (plainLength <= 16) //4x4
                columnCount = ClampNext(root, 2, 4);
            else if (plainLength <= 64)  //8*8
                columnCount = ClampNext(root, 4, 8);
            else if (plainLength <= 256)  //16*16
                columnCount = ClampNext(root, 8, 16);
            else if (plainLength <= 1024)  //32*32
                columnCount = ClampNext(root, 16, 32);
            else if (plainLength <= 4096) //64*64
                columnCount = ClampNext(root, 32, 64);
            return columnCount;
        }

        private static int ClampNext(int next, int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            return (next.UMod(range)) + minValue;
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            //not needed
            throw new NotImplementedException();
        }

        public byte[] EncryptWithFill(byte[] plain, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnCount = GetColumnCount(plain.Length, rng);

            IReadOnlyList<int> filledPlain = BytesToIndicesAndFill(plain, columnCount);

            int[] encryptIndices = Encrypt(filledPlain, columnCount);

            return IndicesToBytes(encryptIndices);
        }

        public byte[] DecryptWithFill(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int fillCount = ByteToIndex(cypher.Last());

            int columnCount = GetColumnCount(cypher.Length - fillCount, rng);

            //TODO: Return List so I can remove fill
            int[] cypherIx = ByteToIndices(cypher);

            //aovid extra allocation to bytes before i remove fill
            int[] plainIx = Decrypt(cypherIx, columnCount);

            return plainIx
                .Take(plainIx.Length - fillCount)
                .Select(IndexToByte)
                .ToArray();
        }

        protected List<int> BytesToIndicesAndFill(byte[] plain, int columnCount, bool extraRow = false)
        {
            //we don't care about these values, it's probably better that
            // we don't use the key-stream to generate them to not leak them at all. 
            // In some way, they will also act as pepper, so that's cool
            IRng rng = new SysRng();
            int remainder = plain.Length % columnCount;
            int rowSize = columnCount;
            int missingEntries = rowSize - remainder; //remainder == 0 ? 0 : rowSize - remainder;
            List<int> plainIndices = plain.Select(ByteToIndex).ToList();

            if (extraRow)
                missingEntries += rowSize;

            for (int i = 0; i < missingEntries - 1; ++i)
                plainIndices.Add(rng.Next(_map.Count));

            Debug.Assert(missingEntries < _map.Count);
            plainIndices.Add(missingEntries);

            return plainIndices;
        }
    }


    /// <summary>
    /// Each column will be assigned a row individually,
    /// instead of 
    /// </summary>
    public class ColumnarTranspositionCypher : TranspositionCypher
    {
        /// <summary>
        /// Use the key as a keyword, nothing fancy
        /// </summary>
        public byte[] EncryptClassic(byte[] plain, byte[] key)
        {
            int columnCount = key.Length;

            int[] plainIx = ByteToIndices(plain);

            List<int> columnOrder = GetColumnOrderFromKey(key);

            byte[] cypher = Encrypt(plain, columnOrder);

            return cypher;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnCount = GetColumnCount(plain.Length, rng);
            List<int> columnOrder = GetColumnOrder(rng, columnCount);

            byte[] cypher = Encrypt(plain, columnOrder);

            return cypher;
        }

        public byte[] Encrypt(byte[] plain, IReadOnlyList<int> columnOrder)
        {
            int[] plainIx = ByteToIndices(plain);

            List<int> cypherIx = Encrypt(plainIx, columnOrder);

            return IndicesToBytes(cypherIx);
        }

        public List<int> Encrypt(IReadOnlyList<int> plainIx, IReadOnlyList<int> columnOrder)
        {
            int columnCount = columnOrder.Count;
            int rowSize = columnCount;

            var order = columnOrder
                .Select((or, colIx) => new { Col = colIx, Order = or })
                .OrderBy(k=> k.Order);

            List<int> cypherIx = new List<int>(plainIx.Count);
            foreach(var o in order)
            {
                for (int j = o.Col; j < plainIx.Count; j += rowSize)
                    cypherIx.Add(plainIx[j]);
            }

            return cypherIx;
        }

        /// <summary>
        /// Use the key as a keyword, nothing fancy
        /// </summary>
        public byte[] DecryptClassic(byte[] cypher, byte[] key)
        {
            int columnCount = key.Length;

            int[] cypherIx = ByteToIndices(cypher);

            List<int> columnOrder = GetColumnOrderFromKey(key);

            byte[] plain = Decrypt(cypher, columnOrder);

            return plain;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnCount = GetColumnCount(cypher.Length, rng);
            List<int> columnOrder = GetColumnOrder(rng, columnCount);

            byte[] plain = Decrypt(cypher, columnOrder);

            return plain;
        }

        public byte[] Decrypt(byte[] cypherTxt, IReadOnlyList<int> columnOrder)
        {
            int[] cypherIx = ByteToIndices(cypherTxt);

            List<int> plainIx = Decrypt(cypherIx, columnOrder);

            return IndicesToBytes(plainIx);
        }

        public List<int> Decrypt(IReadOnlyList<int> cypherIx, IReadOnlyList<int> columnOrder)
        {
            int columnCount = columnOrder.Count;
            int rowSize = columnCount;

            var order = columnOrder
                .Select((or, colIx) => new { Col = colIx, Order = or })
                .OrderBy(k => k.Order);

            List<int> plainIx = Enumerable
                .Range(0, cypherIx.Count)
                .Select(_ => -1)
                .ToList();

            int i = 0;
            foreach (var o in order)
            {
                for (int j = o.Col; j < cypherIx.Count; j += rowSize)
                    plainIx[j] = cypherIx[i++];
            }

            return plainIx;
        }

        private List<int> GetColumnOrderFromKey(byte[] key)
        {
            int[] keyIx = ByteToIndices(key);
            var order = Enumerable.Range(0, key.Length).ToList();
            //all unique indices that appear in key in order
            var ixsInKey = keyIx.Distinct().OrderBy(ix => ix);
            int count = 0;
            foreach (int ixInKey in ixsInKey)
            {
                for (int i = 0; i < order.Count; ++i)
                {
                    if (keyIx[i] == ixInKey)
                        order[i] = count++;
                }
            }

            return order;
        }

        protected static List<int> GetColumnOrder(IRng rng, int columnCount)
        {
            List<int> columnOrder = Enumerable
                .Range(0, columnCount)
                .Select(i => i)
                .ToList();
            columnOrder.Shuffle(rng);
            return columnOrder;
        }
    }

    /// <summary>
    /// If transposition cyphers are all about moving plain text characters around
    /// shuffling them in a determinstic manner is the best you can get
    /// </summary>
    public class ShuffleCypher : TranspositionCypher
    {
        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            IRng rng = key.KeyToRand();
            int[] plainIxs = ByteToIndices(plain);
            var transform = Enumerable
                .Range(0, plainIxs.Length - 1)
                .Select(i => new { old = plain.Length - i - 1, @new = rng.Next(plain.Length - i) })
                .ToList();

            int[] cypherIxs = plainIxs.ToArray();
            foreach(var t in transform)
            {
                int temp = cypherIxs[t.@new];
                cypherIxs[t.@new] = cypherIxs[t.old];
                cypherIxs[t.old] = temp;
            }

            return IndicesToBytes(cypherIxs);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();
            int[] cypherIxs = ByteToIndices(cypher);
            var transform = Enumerable
                .Range(0, cypherIxs.Length - 1)
                .Select(i => new { old = cypher.Length - i - 1, @new = rng.Next(cypher.Length - i) })
                .Reverse()
                .ToList();

            int[] plainIxs = cypherIxs.ToArray();
            foreach (var t in transform)
            {
                int temp = plainIxs[t.old];
                plainIxs[t.old] = plainIxs[t.@new];
                plainIxs[t.@new] = temp;
            }

            return IndicesToBytes(plainIxs);
        }
    }

    public class Maze : Array2D<Maze.Cell>
    {
        protected Maze(int rows, int cols) : base(rows, cols) { }

        public enum Cell : byte
        {
            OpenLeft = 1 << 0,
            OpenTop = 1 << 1,
            OpenRight = 1 << 2,
            OpenBot = 1 << 3,
            Visited = 1 << 4
        }

        public static Maze MakeMaze(int rows, int cols, IRng rng, out List<Tuple<int, int>> visitOrder, bool printSteps = false)
        {
            var arr = new Maze(rows + 2, cols + 2);

            //visit first and last row
            for (int i = 0; i < arr.RowSize; ++i)
            {
                arr[0, i] = Cell.Visited;
                arr[arr.RowCount - 1, i] = Cell.Visited;
            }

            //visit first and last column
            for (int i = 0; i < arr.ColumnSize; ++i)
            {
                arr[i, 0] = Cell.Visited;
                arr[i, arr.ColumnCount - 1] = Cell.Visited;
            }
            visitOrder = new List<Tuple<int, int>>();

            DfsMaze(arr, 1, 1, rng, visitOrder, printSteps);

            visitOrder = visitOrder
                .Select(s => new Tuple<int, int>(s.Item1 - 1, s.Item2 - 1))
                .ToList();

            var res = new Maze(rows, cols);

            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    res[i, j] = arr[i + 1, j + 1];

            if (printSteps)
                Console.WriteLine(res);

            return res;
        }

        private static void DfsMaze(
            Maze cells,
            int i,
            int j,
            IRng rng,
            List<Tuple<int, int>> visitOrder, bool
            print)
        {
            if (print)
                Console.WriteLine(cells);

            visitOrder.Add(new Tuple<int, int>(i, j));
            List<Tuple<int, int, Cell>> neighs = GetUnvistedNeighbours(cells, i, j);
            cells[i, j] |= Cell.Visited;
            while (neighs.Any())
            {
                var pickedNeigh = neighs[rng.Next(0, neighs.Count)];
                int x = pickedNeigh.Item1, y = pickedNeigh.Item2;
                cells[i, j] |= pickedNeigh.Item3;
                cells[x, y] |= GetOpposite(pickedNeigh.Item3);

                DfsMaze(cells, x, y, rng, visitOrder, print);
                neighs = GetUnvistedNeighbours(cells, i, j);
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("╔");
            for (int j = 1; j < ColumnCount; ++j)
            {
                builder.Append("═");
                builder.Append("╦");
            }
            builder.Append("═");
            builder.Append("╗");
            builder.AppendLine();

            for (int j = 0; j < ColumnCount; ++j)
            {
                var cell = this[0, j];
                if (cell.HasFlag(Cell.OpenLeft))
                    builder.Append(" ");
                else
                    builder.Append("║");
                builder.Append(" ");
            }

            builder.Append("║");
            builder.AppendLine();

            for (int i = 1; i < RowCount; ++i)
            {
                builder.Append("╠");
                for (int j = 0; j < ColumnCount; ++j)
                {
                    var cell = this[i, j];
                    if (!cell.HasFlag(Cell.OpenTop))
                        builder.Append("═");
                    else
                        builder.Append(" ");
                    builder.Append("╬");
                }

                builder.AppendLine();

                for (int j = 0; j < ColumnCount; ++j)
                {
                    var cell = this[i, j];
                    if (cell.HasFlag(Cell.OpenLeft))
                        builder.Append(" ");
                    else
                        builder.Append("║");
                    builder.Append(" ");
                }
                builder.Append("║");
                builder.AppendLine();
            }

            builder.Append("╚");
            for (int j = 1; j < ColumnCount; ++j)
            {
                builder.Append("═");
                builder.Append("╩");
            }
            builder.Append("═");
            builder.Append("╝");
            builder.AppendLine();
            return builder.ToString();
        }

        private static List<Tuple<int, int, Cell>> GetUnvistedNeighbours(Array2D<Cell> cells, int i, int j)
        {
            //get unvisited neighbors
            var neigh = new List<Tuple<int, int, Cell>>();

            if (!cells[i - 1, j].HasFlag(Cell.Visited))
                neigh.Add(new Tuple<int, int, Cell>(i - 1, j, Cell.OpenTop));
            if (!cells[i + 1, j].HasFlag(Cell.Visited))
                neigh.Add(new Tuple<int, int, Cell>(i + 1, j, Cell.OpenBot));
            if (!cells[i, j - 1].HasFlag(Cell.Visited))
                neigh.Add(new Tuple<int, int, Cell>(i, j - 1, Cell.OpenLeft));
            if (!cells[i, j + 1].HasFlag(Cell.Visited))
                neigh.Add(new Tuple<int, int, Cell>(i, j + 1, Cell.OpenRight));

            return neigh;
        }

        private static Cell GetOpposite(Cell orig)
        {
            switch (orig)
            {
                case Cell.OpenLeft:
                    return Cell.OpenRight;
                case Cell.OpenTop:
                    return Cell.OpenBot;
                case Cell.OpenRight:
                    return Cell.OpenLeft;
                case Cell.OpenBot:
                    return Cell.OpenTop;
            }
            throw new InvalidOperationException();
        }
    }

    public class MazeCypher : TranspositionCypher
    {

        public override byte[] Encrypt(byte[] plain, int columnCount)
        {
            IRng rng = plain.KeyToRand();
            return Encrypt(plain, columnCount, rng);
        }

        public byte[] Encrypt(byte[] plain, int columnCount, IRng rng)
        { Maze _; return Encrypt(plain, columnCount, rng, out _); }

        public byte[] Encrypt(byte[] plain, int columnCount, IRng rng, out Maze maze)
        {
            int rowCount = plain.Length / columnCount;
            if (plain.Length % columnCount != 0)
                throw new InvalidOperationException();

            List<Tuple<int, int>> order;
            maze = Maze.MakeMaze(rowCount, columnCount, rng, out order);

            byte[] cypher = new byte[plain.Length];
            for (int i = 0; i < plain.Length; ++i)
            {
                var pos = order[i];
                var newIx = pos.Item1 * columnCount + pos.Item2;
                cypher[newIx] = plain[i];
            }

            return cypher;
        }

        public byte[] Decrypt(byte[] cypher, int columnCount, IRng rng)
        {
            int rowCount = cypher.Length / columnCount;
            if (cypher.Length % columnCount != 0)
                throw new InvalidOperationException();

            List<Tuple<int, int>> order;
            var maze = Maze.MakeMaze(rowCount, columnCount, rng, out order);

            byte[] plain = new byte[cypher.Length];
            for (int i = 0; i < cypher.Length; ++i)
            {
                var pos = order[i];
                var newIx = pos.Item1 * columnCount + pos.Item2;
                plain[i] = cypher[newIx];
            }

            return plain;
        }

        //public override byte[] Encrypt(byte[] plain, byte[] key)
        //{
        //    var getCol
        //}

        //public override byte[] Decrypt(byte[] cypher, byte[] key)
        //{
        //    return base.Decrypt(cypher, key);
        //}

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            //not needed
            throw new NotImplementedException();
        }
    }
}
