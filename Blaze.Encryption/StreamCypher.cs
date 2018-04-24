using Blaze.Encryption.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public class StreamCypher : AlphabeticEncrypt
    {
        public StreamCypher() { }

        public StreamCypher(char[] alphabet)
        {
            Alphabet = alphabet;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> op)
        {
            byte[] keyHash = key.GetMD5Hash();
            IRng rand = keyHash.KeyToRand();

            byte[] cypher = Encrypt(plain, rand, op);

            return cypher;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> reverseOp)
        {
            return Encrypt(cypher, key, reverseOp);
        }

        public override byte[] Encrypt(byte[] plain, IRng rand, Func<int, int, int> op)
        {
            var cypher = new byte[plain.Length];

            var p = ByteToIndices(plain);

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
