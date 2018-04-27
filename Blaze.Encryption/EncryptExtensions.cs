using Blaze.Cryptography.Extensions.Operations;
using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public static class EncryptExtenstions
    {
        #region KeyStuff
        public static byte[] GetMD5Hash(this byte[] key, byte[] salt = null)
        {
            byte[] keyHash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var saltedKey = new List<byte>(key);
                if(salt != null)
                    saltedKey.AddRange(salt);
                keyHash = md5.ComputeHash(saltedKey.ToArray());
            }
            return keyHash;
        }

        public static int ToSeed(this byte[] self, bool littleEndian = true)
        {
            int res = 0;
            if (self.Length == sizeof(int))
            {
                //little endian
                if (littleEndian)
                    res = BitConverter.ToInt32(self, 0);
                else //bigendian
                    for (var ii = 0; ii < self.Length; ++ii)
                    {
                        res |= self[self.Length - ii - 1] << (ii * 8);
                    }
            }
            else
            {
                int start = 0;
                while (start < self.Length)
                {
                    if (self.Length - start < 4)
                        break; //not enough bytes left
                    res = res ^ BitConverter.ToInt32(self, start);
                    start += 4;
                }

                if (start != self.Length)
                {
                    var remaning = self.Skip(start).Take(self.Length - start).ToList();
                    int zeroes = 4 - (self.Length - start);
                    System.Diagnostics.Trace.Assert(zeroes > 0 && zeroes < 4);
                    for (var ii = 0; ii < zeroes; ++ii)
                        remaning.Add(0);
                    res = res ^ BitConverter.ToInt32(remaning.ToArray(), 0);
                }
            }
            return res;

        }

        public static IRng KeyToRand(this byte[] key)
        {
            int seed = key.ToSeed();
            var rand = new SysRng(seed);
            return rand;
        }

        public static byte[] Pepper(this byte[] key, byte[] pepper)
        {
            byte[] final = new byte[key.Length];
            for (int i = 0; i < key.Length; ++i)
                final[i] = (byte)(key[i] ^ pepper[i]);
            return final;
        }
        #endregion

        public static UInt64 LongRandom(this IRng rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            UInt64 longRand = BitConverter.ToUInt64(buf, 0);
            return longRand;
        }

        public static void Shuffle<T>(this IList<T> list, IRng rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
