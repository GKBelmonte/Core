using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography
{
    /// <summary>
    /// I will call all nonce IVs salt, because the concept is almost the
    /// same and I enjoy the cooking analogy
    /// </summary>
    public class SaltedCypher : AlphabeticCypher
    {
        int _saltLength;
        ICypher _cypher;
        public SaltedCypher(ICypher inner, int saltLength = 20)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            _cypher = inner;
            _saltLength = saltLength;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            byte[] salt, newKey;
            salt = PinchOfSalt(_saltLength);
            SaltKey(key, salt, out newKey);

            byte[] encodedSalt = IndicesToBytes(EncodeBytes(salt, Alphabet.Count).ToList());

            byte[] res = _cypher.Encrypt(plain, newKey);
            
            return encodedSalt.Concat(res).ToArray();
        }

        public override byte[] Decrypt(byte[] plain, byte[] key)
        {
            byte[] salt, newKey;
            int skipCount;
            salt = DecodeBytes(BytesToIndices(plain), Alphabet.Count, out skipCount);

            SaltKey(key, salt, out newKey);

            byte[] res = _cypher.Decrypt(plain.Skip(skipCount).ToArray(), newKey);
            return res;
        }

        public override IReadOnlyList<char> Alphabet
        {
            get { return _cypher.Alphabet; }
            set
            {
                base.Alphabet = value;
                if(_cypher != null)
                    _cypher.Alphabet = value;
            }
        }

        public override Op ForwardOp
        {
            get { return _cypher.ForwardOp; }
            set { if (_cypher != null) _cypher.ForwardOp = value; }
        }

        public override Op ReverseOp
        {
            get => _cypher.ReverseOp;
            set { if (_cypher != null) _cypher.ReverseOp = value; }
        }

        private static byte[] PinchOfSalt(int saltLength)
        {
            byte[] salt = new byte[saltLength];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetNonZeroBytes(salt);
            return salt;
        }

        private static void SaltKey(byte[] key, byte[] salt, out byte[] newKey)
        {
            bool keyIsLonger = key.Length > salt.Length;

            byte[] longSequence, shortSequence;

            if (keyIsLonger)
            {
                longSequence = key;
                shortSequence = salt;
                newKey = new byte[key.Length];
            }
            else
            {
                longSequence = salt;
                shortSequence = key;
                newKey = new byte[salt.Length];
            }

            int i = 0;

            for (; i < shortSequence.Length; ++i)
                newKey[i] = (byte)(shortSequence[i] ^ longSequence[i]);

            for (; i < longSequence.Length; ++i)
                newKey[i] = longSequence[i];
        }


        /// <summary>
        /// Encodes the bytes in a base 'numBase'.
        /// Usefull if the alphabet size is less than 256.
        /// </summary>
        public static IEnumerable<int> EncodeBytes(byte[] buff, int numBase, int numOfdigitsToEncodeLength = 2)
        {
            int digitsPerByte = (int)Math.Ceiling(Math.Log(256, numBase));
            //we'll use 2 digits to represent the number of bytes
            int maxEncodableLength = (int)Math.Pow(numBase, numOfdigitsToEncodeLength) - 1;
            // e.g.: alphabet length  max encodable length (2 digits)
            //          10                  99
            //          20                  399
            //          26                  675
            //          32                  1023
            if (buff.Length > maxEncodableLength)
                throw new InvalidOperationException($"Can't encode {buff.Length} bytes in base {numBase}. The max encodable length is {maxEncodableLength}");
            foreach (int i in EncodeInt(buff.Length, numBase, numOfdigitsToEncodeLength))
                yield return i;

            foreach (byte b in buff)
                foreach (int j in EncodeInt(b, numBase, digitsPerByte))
                    yield return j;
        }

        public static IEnumerable<int> EncodeInt(int b, int numBase, int digits)
        {
            for (int i = 0; i < digits; ++i)
            {
                yield return b % numBase;
                b = b / numBase; 
            }
        }

        public static byte[] DecodeBytes(IReadOnlyList<int> vals, int numBase, out int skipCount, int numOfdigitsToEncodeLength = 2)
        {
            int digitsPerByte = (int)Math.Ceiling(Math.Log(256, numBase));
            int byteCount = DecodeInt(vals, numBase, numOfdigitsToEncodeLength);
            byte[] res = new byte[byteCount];
            for (int i = 0; i < byteCount; ++i)
            {
                int beg = i * digitsPerByte + numOfdigitsToEncodeLength;
                int end = (i + 1) * digitsPerByte + numOfdigitsToEncodeLength;
                var span = new ListSpan<int>(vals, beg, end);
                res[i] = (byte)DecodeInt(span, numBase, digitsPerByte);
            }

            skipCount = numOfdigitsToEncodeLength + digitsPerByte * byteCount;

            return res;
        }

        public static int DecodeInt(IReadOnlyList<int> digits, int numBase, int digitCount)
        {
            int res = 0, pow = 1;
            for (int i = 0; i < digitCount; ++i)
            {
                res += digits[i] * pow;
                pow *= numBase;
            }
            return res;
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            throw new NotImplementedException();
        }
    }
}
