using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;

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

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnCount = GetColumnCount(cypher.Length, rng);

            byte[] plain = Decrypt(cypher, columnCount);

            return plain;
        }

        public byte[] Encrypt(byte[] plain, int columnCount)
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

        public byte[] Decrypt(byte[] cypher, int columnCount)
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
            int missingEntries = rowSize - remainder;
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
            List<int> plainIx = BytesToIndicesAndFill(plain, columnOrder.Count, true);

            List<int> cypherIx = Encrypt(plainIx, columnOrder);

            int remainder = plain.Length % columnOrder.Count;
            int rowSize = columnOrder.Count;
            int missingEntries = rowSize - remainder;
            //I dont know a better way, I can't revert GetColumnCount
            cypherIx[cypherIx.Count - 1] = missingEntries;

            return IndicesToBytes(cypherIx);
        }

        public List<int> Encrypt(IReadOnlyList<int> plainIx, IReadOnlyList<int> columnOrder)
        {
            int columnCount = columnOrder.Count;
            int rowSize = columnCount;
            int columnSize = plainIx.Count / rowSize;


            var cypherIx = new List<int>(plainIx.Count);
            foreach (int column in columnOrder)
            {
                for (int i = 0; i < columnSize; ++i)
                {
                    cypherIx.Add(plainIx[column + i*rowSize]);
                }
            }

            return cypherIx;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();

            //Dammit. 
            //In order to know the number of columns i need the plain-length AND
            // the rng, 
            // but i order to know the plain-length i need the number of columns AND
            // the rng...

            byte[] plain = Decrypt(cypher, 3);

            return plain;
        }

        public byte[] Decrypt(byte[] cypherTxt, IReadOnlyList<int> columnOrder)
        {
            throw new NotImplementedException();
        }

        public List<int> Decrypt(IReadOnlyList<int> cypherIxs, IReadOnlyList<int> columnOrder)
        {
            throw new NotImplementedException();
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

}
