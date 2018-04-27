using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Classics
{
    public class TranspositionCypher : AlphabeticCypher
    {
        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnNumber = GetColumnNumber(plain, rng);

            byte[] cypher = Encrypt(plain, columnNumber);

            return cypher;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();

            int columnNumber = GetColumnNumber(cypher, rng);

            byte[] plain = Decrypt(cypher, columnNumber);

            return plain;
        }

        public byte[] Encrypt(byte[] plain, int columnNumber)
        {
            int[] plainIx = ByteToIndices(plain);

            int[] cypher = Encrypt(plainIx, columnNumber);

            return IndicesToBytes(cypher).ToArray();
        }

        private int[] Encrypt(int[] plainInts, int columnNumber)
        {
            int rowSize = columnNumber;
            
            List<int> cypher = new List<int>(plainInts.Length);
            for (int i = 0; i < rowSize; ++i)
            {
                for (int j = i; j < plainInts.Length; j += rowSize)
                    cypher.Add(plainInts[j]);
            }
            return cypher.ToArray();
        }

        public byte[] Decrypt(byte[] cypher, int columnNumber)
        {
            int[] cypherIx = ByteToIndices(cypher);

            int[] plainIx = Decrypt(cypherIx, columnNumber);

            return IndicesToBytes(plainIx).ToArray();
        }

        private int[] Decrypt(int[] cypherIx, int originalColumnNumber)
        {
            // new columnSize is old columnNumber
            int columnSize = originalColumnNumber;
            int rowNumber = columnSize;
            int remainder = cypherIx.Length % originalColumnNumber;
            //remainder == originally complete Columns / complete Rows
            int completeRows = remainder == 0 ? rowNumber : remainder;
            
            //new rowSize is length / original column number
            // rowSize is number of columns
            int rowSize = cypherIx.Length / originalColumnNumber + (remainder == 0 ? 0 : 1);
            int columnNumber = rowSize;

            List<int> plainIx = new List<int>(cypherIx.Length);
            for (int i = 0; i < columnNumber; ++i)
            {
                int rowCount = 0;
                int j = i;
                //Get the indexes (j) where the rows are complete (+=rowSize)
                for (; rowCount < completeRows && j < cypherIx.Length; ++rowCount)
                {
                    plainIx.Add(cypherIx[j]);
                    j += rowSize;
                }

                //For the last column, we only want complete rows
                if (i == columnNumber - 1)
                    continue;

                //Get the indexes (j) where the rows are incomplete (+=rowSize -1 )
                for(; rowCount < rowNumber && j < cypherIx.Length; ++rowCount)
                {
                    plainIx.Add(cypherIx[j]);
                    j += rowSize - 1;
                }
            }

            return plainIx.ToArray();
        }

        private static int GetColumnNumber(byte[] plain, IRng rng)
        {
            int columnNumber = 0;
            if (plain.Length < 16)
                columnNumber = rng.Next(3, 6);
            else if (plain.Length < 32)
                columnNumber = rng.Next(4, 8);
            else if (plain.Length < 128)
                columnNumber = rng.Next(9, 15);
            else if (plain.Length < 1024)
                columnNumber = rng.Next(24, 40);
            else if (plain.Length < 65536)
                columnNumber = rng.Next(230, 280);
            return columnNumber;
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            //not needed
            throw new NotImplementedException();
        }
    }
}
