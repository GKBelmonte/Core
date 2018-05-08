using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography
{
    public class BifidCypher : AlphabeticCypher
    {
        public override IReadOnlyList<char> Alphabet
        {
            get { return base.Alphabet; }
            set
            {
                if (!Blaze.Core.Math.Utils.IsPerfectSquare(value.Count))
                    throw new InvalidOperationException($"{nameof(BifidCypher)} alphabet size must be a perfect square");
                base.Alphabet = value;
            }
        }

        private Array2D<int> GetSeededPolybiusSquare(IRng rng)
        {            
            List<int> ixs = Alphabet.Select((_, i) => i).ToList();
            ixs.Shuffle(rng);

            int sqrtSize = (int)Math.Sqrt(Alphabet.Count);
            
            var res = new Array2D<int>(sqrtSize, sqrtSize);

            for (int i = 0; i < sqrtSize; ++i)
                for (int j = 0; j < sqrtSize; ++j)
                    res[i, j] = ixs[i * sqrtSize + j];

            return res;
        }

        private Dictionary<int, Tuple<int, int>> GetPolybiusLookup(Array2D<int> polybiusSquare)
        {
            var res = new Dictionary<int, Tuple<int, int>>();
            int sqrtSize = polybiusSquare.ColumnCount;
            for (int i = 0; i < sqrtSize; ++i)
                for (int j = 0; j < sqrtSize; ++j)
                    res.Add(polybiusSquare[i, j], new Tuple<int, int>(i, j));
            return res;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            IRng rng = key.KeyToRand();
            Array2D<int> polybiusSquare = GetSeededPolybiusSquare(rng);
            return Encrypt(plain, polybiusSquare);
        }

        public byte[] Encrypt(byte[] plain, Array2D<int> polybiusSquare)
        {
            Dictionary<int, Tuple<int, int>> lookup = GetPolybiusLookup(polybiusSquare);

            int[] plainIxs = ByteToIndices(plain);

            var polySub = new List<int>(plainIxs.Length * 2);
            foreach (int ix in plainIxs)
            {
                var coords = lookup[ix];
                polySub.Add(coords.Item1);
                polySub.Add(coords.Item2);
            }

            var cypherIxs = new List<int>(plainIxs.Length);
            int rowSize = 2;
            for (int i = 0; i < rowSize; ++i)
            {
                for (int j = i; j < polySub.Count; j += 2 * rowSize)
                {
                    int cypherIx = polybiusSquare[polySub[j], polySub[j + rowSize]];
                    cypherIxs.Add(cypherIx);
                }
            }

            return IndicesToBytes(cypherIxs);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            IRng rng = key.KeyToRand();
            Array2D<int> polybiusSquare = GetSeededPolybiusSquare(rng);
            return Decrypt(cypher, polybiusSquare);
        }

        private byte[] Decrypt(byte[] cypher, Array2D<int> polybiusSquare)
        {
            Dictionary<int, Tuple<int, int>> lookup = GetPolybiusLookup(polybiusSquare);

            int[] cypherIxs = ByteToIndices(cypher);

            var polySub = new List<int>(cypherIxs.Length * 2);
            foreach (int ix in cypherIxs)
            {
                var coords = lookup[ix];
                polySub.Add(coords.Item1);
                polySub.Add(coords.Item2);
            }

            var plainIxs = new List<int>(cypherIxs.Length);
            int rowSize = polySub.Count / 2;
            for (int i = 0; i < rowSize; ++i)
            {
                for (int j = i; j < polySub.Count; j += 2 * rowSize)
                {
                    int plainIx = polybiusSquare[polySub[j], polySub[j + rowSize]];
                    plainIxs.Add(plainIx);
                }
            }

            return IndicesToBytes(plainIxs);
        }

        public Array2D<int> GetPolybiusSquareFromBytes(byte[] polybiusSquareBytes)
        {
            int sqrtSize = (int)Math.Sqrt(polybiusSquareBytes.Length);
            Array2D<int> polybiusSquare = new Array2D<int>(sqrtSize, sqrtSize);

            var polybiusSquareIxs = ByteToIndices(polybiusSquareBytes); 

            for (int i = 0; i < sqrtSize; ++i)
                for (int j = 0; j < sqrtSize; ++j)
                    polybiusSquare[i, j] = polybiusSquareIxs[i * sqrtSize + j];

            return polybiusSquare;
        }

        public byte[] EncryptClassic(byte[] plain, byte[] polybiusSquareBytes)
        {
            if (!Core.Math.Utils.IsPerfectSquare(polybiusSquareBytes.Length))
                throw new ArgumentException($"{nameof(polybiusSquareBytes)} must be a perfect square");
            //kinda backwards...
            Alphabet = polybiusSquareBytes.Select(b => (char)b).ToList();

            return Encrypt(plain, GetPolybiusSquareFromBytes(polybiusSquareBytes));
        }

        public byte[] DecrpytClassic(byte[] plain, byte[] polybiusSquareBytes)
        {
            if (!Core.Math.Utils.IsPerfectSquare(polybiusSquareBytes.Length))
                throw new ArgumentException($"{nameof(polybiusSquareBytes)} must be a perfect square");
            //kinda backwards...
            Alphabet = polybiusSquareBytes.Select(b => (char)b).ToList();

            return Decrypt(plain, GetPolybiusSquareFromBytes(polybiusSquareBytes));
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            throw new InvalidOperationException("unused");
        }

        //TODO: move post merge
        public static void WikiTest()
        {
            byte[] plainText = "FLEEATONCE".ToByteArray();
            byte[] polybiusSq = @"B G W K Z
                                 Q P N D S
                                 I O A X E
                                 F C L U M
                                 T H Y V R"
                .Replace(" ", string.Empty)
                .Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .ToByteArray();

            var bifid = new BifidCypher();
            var cypherBytes = bifid.EncryptClassic(plainText, polybiusSq);
            string cypher = cypherBytes.ToTextString();
            bool ok = cypher == "U  A  E  O  L  W  R  I  N  S".Replace(" ", string.Empty);

            string decypher = bifid.DecrpytClassic(cypherBytes, polybiusSq)
                .ToTextString();
        }
    }
}
