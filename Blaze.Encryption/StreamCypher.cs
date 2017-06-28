using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encryption
{
    public class StreamCypher : AlphabeticEncrypt, IOperationEncrypt, ISeededEncrypt
    {
        public StreamCypher() { }

        public int BlockSize { get; set; }

        public StreamCypher(char[] alphabet)
        {
            Alphabet = alphabet;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {
            byte[] keyHash = ProcessKeyInternal(key);
            var rand = keyHash.KeyToRand();

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

        public override byte[] Decrypt(byte[] cypher, byte[] key, Operation op)
        {
            return Encrypt(cypher, key, op);
        }

        public Random Rand { get; set; }
    }
}
