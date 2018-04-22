using Blaze.Encryption.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public class StreamCypher : SeededEncryptBase, IOperationEncrypt, ISeededEncrypt
    {
        public StreamCypher() { }

        public StreamCypher(char[] alphabet)
        {
            Alphabet = alphabet;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {
            byte[] keyHash = key.GetMD5Hash();
            IRng rand = keyHash.KeyToRand();

            byte[] cypher = Encrypt(plain, rand, op);

            return cypher;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Operation op)
        {
            return Encrypt(cypher, key, op);
        }

        private byte[] Encrypt(byte[] plain, IRng rand, Operation op)
        {
            var cypher = new byte[plain.Length];
            var f = GetOpFunc(op);
            var p = ByteToIndices(plain);

            for (var ii = 0; ii < plain.Length; ++ii)
            {
                //support more block sizes?
                int plainInx = p[ii];
                int nextInx = rand.Next();
                cypher[ii] = IndexToByte(f(plainInx, nextInx));
            }

            return cypher;
        }

        public override byte[] Encrypt(byte[] plain, IRng key)
        {
            return Encrypt(plain, key, Operation.Xor);
        }

        public override byte[] Decrypt(byte[] cypher, IRng key)
        {
            return Encrypt(cypher, key, Operation.Xor);
        }
    }
}
