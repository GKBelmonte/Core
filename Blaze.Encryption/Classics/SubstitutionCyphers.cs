using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Classics
{
    public class Vigenere : AlphabeticCypher, ICypher
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

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            byte[] cypher = new byte[plain.Length];
            int inx = -1;

            int[] keyIndices = BytesToIndices(key);
            int[] textIndices = BytesToIndices(plain);

            while (++inx < cypher.Length)
            {
                int keyInx = keyIndices[inx % key.Length];
                int plainInx = textIndices[inx];
                cypher[inx] = IndexToByte(op(plainInx, keyInx));
            }
            return cypher;
        }
    }

    public class CaesarCypher : Vigenere
    {
        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            uint shortKey = (uint)(key.Length > 1 ? key.ToInt32() : key[0]);
            byte k = 0;
            for (int ii = 0; ii < 4; ++ii)
                k ^= (byte)((shortKey >> (ii * 8)) & 0xFF);

            return base.Encrypt(plain, new[] {k}, op);
        }

        protected byte[] Encrypt(byte[] plain, byte key, Op op)
        {
            return base.Encrypt(plain, new[] { key }, op);
        }
    }

    public class Rot13 : CaesarCypher
    {
        public Rot13()
        {
            Alphabet = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char) i).ToArray();
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {   
            return Encrypt(plain, (byte)('A' + 13), op);
        }
    }
}
