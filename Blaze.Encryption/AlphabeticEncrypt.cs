using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;

namespace Blaze.Encryption
{
    /// <summary>
    /// Base class for having a restricted alphabet
    /// By default, we use all of 0-255 
    /// (even though C# encodes in unicode, utf-16, but meh, once encrypted we generally don't care
    /// if the string encoding is broken or we use just bytes)
    /// </summary>
    public abstract class AlphabeticEncrypt : IOperationEncrypt
    {
        protected AlphabeticEncrypt()
        {
            InitializeDefaultAlphabet();
        }

        /// <summary>
        /// An alphabet is a set of characters whose representation
        /// (be it ASCII or UTF-8) might have gaps and holes in between.
        /// Indices are the position of that character in their alphabet
        /// regardless of encoding.
        /// So the first letter of the alphabet is index 0, regardless
        /// if it is 'A'
        /// </summary>
        protected Map<int, byte> _Map;

        protected char[] _Alphabet;
        public virtual char[] Alphabet
        {
            get { return _Alphabet; }
            set
            {
                _Alphabet = value;
                _Map = new Map<int, byte>();
                for (var ii = 0; ii < Alphabet.Length; ++ii)
                {
                    byte b = (byte)Alphabet[ii];
                    _Map.Add(ii, b);
                }
            }
        }

        protected void InitializeDefaultAlphabet()
        {
            _Alphabet = new char[256];
            for (var ii = 0; ii < 256; ++ii)
                _Alphabet[ii] = (char)ii;

            Alphabet = _Alphabet;
        }

        protected int ByteToIndex(byte nomnom)
        {
            return _Map.Reverse[nomnom];
        }

        protected int[] ByteToIndices(byte[] buff)
        {
            var keyIndices = new int[buff.Length];
            for (var ii = 0; ii < buff.Length; ++ii)
                keyIndices[ii] = ByteToIndex(buff[ii]);

            return keyIndices;
        }

        protected byte IndexToByte(int inx)
        {
            return _Map.Forward[inx.UMod(_Map.Count)];
        }

        protected byte[] IndicesToBytes(int[] indices)
        {
            var bytes = new byte[indices.Length];
            for (var ii = 0; ii < indices.Length; ++ii)
                bytes[ii] = IndexToByte(indices[ii]);

            return bytes;
        }

        public Func<int, int, int> CustomOp { get; set; }
        public Func<int, int, int> ReverseOp { get; set; }

        public Func<int, int, int> GetOpFunc(Operation op)
        {
            Func<int, int, int> d = null;
            switch (op)
            {
                case Operation.Add:
                    d = (a, b) => a + b;
                    break;
                case Operation.Sub:
                    d = (a, b) => a - b;
                    break;
                case Operation.Xor:
                    d = (a, b) => a ^ b;
                    break;
                case Operation.Custom:
                    if (CustomOp == null)
                        throw new InvalidOperationException("The Custom Operation has not been assigned!");
                    return CustomOp;
                case Operation.ReverseCustom:
                    if (ReverseOp == null)
                        throw new InvalidOperationException("The Reverse Custom Operation has not been assigned!");
                    return ReverseOp;
            }
            return d;
        }

        public static char[] GetSimpleAlphabet(bool caps = true, bool lower = true, bool space = true, bool nullChar = false)
        {
            List<char> alphabet = new List<char>();
            if (caps)
                for (char ii = 'A'; ii <= 'Z'; ++ii)
                    alphabet.Add(ii);

            if (space)
                alphabet.Add(' ');

            if (lower)
                for (char ii = 'a'; ii <= 'z'; ++ii)
                    alphabet.Add(ii);

            if (nullChar)
                alphabet.Add('\0');

            return alphabet.ToArray();
        }

        public static char[] GetPowerOf2Alphabet()
        {
            var alpha = AlphabeticEncrypt.GetSimpleAlphabet(false);
            var powOf2Alphabet = new List<char>(alpha);
            powOf2Alphabet.AddRange(new char[] { 'A', 'H', 'B', 'W', 'N' });
            return powOf2Alphabet.ToArray();
        }

        public static char[] GetPlainTextAlphabet()
        {
            List<char> plainTextChars = new List<char>(128);
            plainTextChars.Add('\n');

            for (int a = 0; a < 256; ++a)
            {
                char c = (char)a;
                if (!char.IsControl(c))
                    plainTextChars.Add(c);
                if (plainTextChars.Count == 128)
                    break;
            }
            return plainTextChars.ToArray();
        }

        public abstract byte[] Encrypt(byte[] plain, byte[] key, Operation op);

        public abstract byte[] Decrypt(byte[] cypher, byte[] key, Operation op);

        public virtual byte[] Encrypt(byte[] plain, byte[] key)
        {
            return Encrypt(plain, key, Operation.Add);
        }

        public virtual byte[] Decrypt(byte[] cypher, byte[] key)
        {
            return Decrypt(cypher, key, Operation.Sub);
        }
    }
}
