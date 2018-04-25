using System;
using System.Collections.Generic;
using System.Linq;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography
{
    /// <summary>
    /// Base class for having a restricted alphabet
    /// By default, we use all of 0-255 
    /// (even though C# encodes in unicode, utf-16, but meh, once encrypted we generally don't care
    /// if the string encoding is broken or we use just bytes)
    /// </summary>
    public abstract class AlphabeticCypher : ICypher
    {
        protected AlphabeticCypher()
        {
            InitializeDefaultAlphabet();
        }

        public abstract byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> op);

        public virtual byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> reverseOp)
        {
            //Good enough for Reciprocal  cyphers where op^(-1) is all you need
            // i.e.: The Encrypt is an involution
            //Fibonnacci, Transposition and Chain will need to override tho
            return Encrypt(cypher, key, reverseOp);
        }


        public virtual byte[] Encrypt(byte[] plain, IRng key, Func<int, int, int> op)
        {
            //Well let's make something work, yes?
            byte[] keyGen = new byte[128];
            key.NextBytes(keyGen);
            return Encrypt(plain, keyGen, op);
        }

        public virtual byte[] Decrypt(byte[] cypher, IRng key, Func<int, int, int> reverseOp)
        {
            byte[] keyGen = new byte[128];
            key.NextBytes(keyGen);
            return Decrypt(cypher, keyGen, reverseOp);
        }

        /// <summary>
        /// An alphabet is a set of characters whose representation
        /// (be it ASCII or UTF-8) might have gaps and holes in between.
        /// Indices are the position of that character in their alphabet
        /// regardless of encoding.
        /// So the first letter of the alphabet is index 0, regardless
        /// if it is 'A'
        /// </summary>
        protected Map<int, byte> _map;

        protected IReadOnlyList<char> _alphabet;
        public virtual IReadOnlyList<char> Alphabet
        {
            get { return _alphabet; }
            set
            {
                _alphabet = value.ToArray();
                _map = new Map<int, byte>();
                for (var ii = 0; ii < Alphabet.Count; ++ii)
                {
                    byte b = (byte)Alphabet[ii];
                    _map.Add(ii, b);
                }
            }
        }

        protected void InitializeDefaultAlphabet()
        {
            var alphabet = new char[256];
            for (var ii = 0; ii < 256; ++ii)
                alphabet[ii] = (char)ii;

            Alphabet = alphabet;
        }

        protected int ByteToIndex(byte nomnom)
        {
            return _map.Reverse[nomnom];
        }

        protected int[] ByteToIndices(IList<byte> buff)
        {
            var keyIndices = new int[buff.Count];
            for (var ii = 0; ii < buff.Count; ++ii)
                keyIndices[ii] = ByteToIndex(buff[ii]);

            return keyIndices;
        }

        protected byte IndexToByte(int inx)
        {
            return _map.Forward[inx.UMod(_map.Count)];
        }

        protected byte[] IndicesToBytes(IList<int> indices)
        {
            var bytes = new byte[indices.Count];
            for (var ii = 0; ii < indices.Count; ++ii)
                bytes[ii] = IndexToByte(indices[ii]);

            return bytes;
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
            var alpha = AlphabeticCypher.GetSimpleAlphabet(false);
            var powOf2Alphabet = new List<char>(alpha);
            powOf2Alphabet.AddRange(new char[] { 'A', 'H', 'B', 'W', 'N' });
            return powOf2Alphabet.ToArray();
        }



        private static IReadOnlyList<char> _plainTextAlphabet;
        /// <summary>
        /// Gets a plain text alphabet that contains exactly 128 characters
        /// the first 126 non-control characters and \r and \n
        /// </summary>
        public static IReadOnlyList<char> GetPlainTextAlphabet()
        {
            if (_plainTextAlphabet != null)
                return _plainTextAlphabet;

            char[] plainTextChars = new char[128];
            int charCount = 0;
            plainTextChars[charCount++] = '\r';
            plainTextChars[charCount++] = '\n';
            
            for (int a = 0; a < 256; ++a)
            {
                char c = (char)a;
                if (!char.IsControl(c))
                    plainTextChars[charCount++] = c;
                if (charCount == 128)
                    break;
            }

            _plainTextAlphabet = plainTextChars;
            return _plainTextAlphabet;
        }

        public static bool IsTextPlain(string text)
        {
            IReadOnlyList<char> plainTextAlpha = GetPlainTextAlphabet();
            bool[] isPlainChar = new bool[256];
            foreach(char c in plainTextAlpha)
            {
                isPlainChar[(byte)c] = true;
            }

            return text.All(
                c => isPlainChar[(byte)c]);
        }
    }
}
