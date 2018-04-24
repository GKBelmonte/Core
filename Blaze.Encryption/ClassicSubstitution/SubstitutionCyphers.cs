using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
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

        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> d)
        {
            byte[] cypher = new byte[plain.Length];
            int inx = -1;

            int[] keyIndices = ByteToIndices(key);
            int[] textIndices = ByteToIndices(plain);

            while (++inx < cypher.Length)
            {
                int keyInx = keyIndices[inx % key.Length];
                int plainInx = textIndices[inx];
                cypher[inx] = IndexToByte(d(plainInx, keyInx));
            }
            return cypher;
        }
    }

    public class CaesarCypher : Vigenere
    {
        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> op)
        {
            byte k = key.Length > 1 ? (byte)(key.ToSeed() % 256) : key[0];
            return base.Encrypt(plain, new[] {k}, op);
        }

        public byte[] Encrypt(byte[] plain, byte key, Func<int, int, int> op)
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

        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> op)
        {   
            return Encrypt(plain, (byte)('A' + 13), op);
        }
    }
}
