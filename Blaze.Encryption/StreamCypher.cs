using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public class StreamCypher : AlphabeticCypher
    {
        public StreamCypher() { }

        public StreamCypher(char[] alphabet)
        {
            Alphabet = alphabet;
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            IRng rand = key.KeyToRand();

            byte[] cypher = Encrypt(plain, rand, op);

            return cypher;
        }

        protected virtual byte[] Encrypt(byte[] plain, IRng rand, Op op)
        {
            var cypher = new byte[plain.Length];

            var p = BytesToIndices(plain);

            for (var ii = 0; ii < plain.Length; ++ii)
            {
                //support more block sizes?
                int plainInx = p[ii];
                int nextInx = rand.Next();
                cypher[ii] = IndexToByte(op(plainInx, nextInx));
            }

            return cypher;
        }
    }
}
