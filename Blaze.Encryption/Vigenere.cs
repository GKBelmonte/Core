using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public class Vigenere : AlphabeticEncrypt, IOperationEncrypt
    {

        public Vigenere()
        {
            InitializeDefaultAlphabet();
        }

        public Vigenere(char[] alphabet)
        {
            Alphabet = alphabet;
        }

        public byte[] EncryptFast(byte[] plain, byte[] key)
        {
            //assume full unchanged alphabet, so no need to do many mod ops
            byte[] cypher = new byte[plain.Length];
            int inx = 0;
            while (inx++ < cypher.Length)
            {
                cypher[inx] = (byte)((plain[inx] + key[inx % key.Length]) % 256);
            }
            return cypher;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            return Encrypt(plain, key, Operation.Add);
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {
            byte[] cypher = new byte[plain.Length];
            int inx = -1;

            Func<int, int, int> d = GetOpFunc(op);

            int[] keyIndices = ByteToIndices(key);
            int[] textIndices = ByteToIndices(plain);

            while (++inx < cypher.Length)
            {
                int keyInx = keyIndices[inx % key.Length];
                int plainInx = textIndices[inx];
                cypher[inx] = IndexToByte(d(plainInx,keyInx));
            }
            return cypher;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Operation op)
        {
            return Encrypt(cypher, key, op);
        }
    }

    public class CaesarCypher : Vigenere
    {
        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {
            byte k = (byte)(key.ToSeed() % 256);
            return base.Encrypt(plain, new[] {k}, op);
        }
    }

    public class Rot13 : CaesarCypher
    {
        public Rot13()
        {
            Alphabet = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char) i).ToArray();
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {   
            return base.Encrypt(plain, new[] { (byte)('A' + 13) }, op);
        }
    }
}
