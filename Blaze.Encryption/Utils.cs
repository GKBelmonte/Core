using Blaze.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public static class CryptoUtils
    {
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
    }
}
