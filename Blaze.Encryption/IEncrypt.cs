using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public interface IEncrypt
    {
        byte[] Encrypt(byte[] plain, byte[] key);
        byte[] Decrypt(byte[] cypher, byte[] key);

        string Encrypt(string plain, string key);
        string Decrypt(string cypher, string key);
    }

    public interface IOperationEncrypt : IEncrypt
    {
        byte[] Encrypt(byte[] plain, byte[] key, Operation op);
        byte[] Decrypt(byte[] cypher, byte[] key, Operation op);

        string Encrypt(string plain, string key, Operation op);
        string Decrypt(string cypher, string key, Operation op);
    }

    //Does the operation on the plain, ignoring the key (treating as 0)
    public class NullCypher : AlphabeticEncrypt, IOperationEncrypt
    {
        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {
            var f = GetOpFunc(op);
            var pIx = ByteToIndices(plain);
            var cx = pIx.Select(px => f(0, px)).ToArray();
            return IndicesToBytes(pIx);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Operation op)
        {
            return Encrypt(cypher, key, op.GetReverse());
        }
    }


    public enum Operation
    {
        Add, Sub, Xor, Custom, ReverseCustom
    }

    public static class EncryptExtenstions
    {

        public static int ToSeed(this byte[] self, bool littleEndian = true)
        {
            int res = 0;
            if (self.Length == sizeof (int))
            {
                //little endian
                if (littleEndian)
                    res = BitConverter.ToInt32(self, 0);
                else //bigendian
                    for (var ii = 0; ii < self.Length; ++ii)
                    {
                        res |= self[self.Length - ii - 1] << (ii*8);
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

        public static UInt64 LongRandom(this Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            UInt64 longRand = BitConverter.ToUInt64(buf, 0);
            return longRand;
        }

        public static Random KeyToRand(this byte[] key)
        {   
            int seed = key.ToSeed();
            var rand = new Random(seed);
            return rand;
        }

        public static Operation GetReverse(this Operation op)
        {
            switch (op)
            {
                case Operation.Add:
                    return Operation.Sub;
                case Operation.Sub:
                    return Operation.Add;
                case Operation.Xor:
                    return Operation.Xor;
                case Operation.Custom:
                    return Operation.ReverseCustom;
                case Operation.ReverseCustom:
                    return Operation.Custom;
                default:
                    throw new InvalidOperationException(string.Format("Cannot reverse unknown op '{0}'", op));
            }
        }
    }
}
