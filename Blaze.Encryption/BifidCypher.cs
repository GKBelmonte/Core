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
            int mod = polySub.Count - 1;
            for (int c = 0; c < plainIxs.Length; ++c)
            {
                int i = (c * 4) % mod;
                int j = i + 2 == mod ? i + 2 : (i + 2) % mod ;
                int cypherIx = polybiusSquare[polySub[i], polySub[j]];
                cypherIxs.Add(cypherIx);
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

            for (int c = 0; c < cypherIxs.Length; ++c)
            {
                int plainIx = polybiusSquare[polySub[c], polySub[c+rowSize]];
                plainIxs.Add(plainIx);
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
    }
}
