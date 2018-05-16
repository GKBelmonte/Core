using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;

namespace Blaze.Cryptography.Rng
{
    public abstract class Int32Rng : IRng
    {
        public abstract int Next();

        public int Next(int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            int next = Next();
            return (next.UMod(range)) + minValue;
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public virtual void NextBytes(byte[] buffer)
        {
            //Cast to avoid sign extensions when shiftting
            uint next = (uint)Next();
            //we could do Next(0,256) and that would discard 3/4 bytes
            byte consumedCount = 0;
            for (int i = 0; i < buffer.Length; ++i)
            {
                byte b = (byte)(next & 0xFF);
                buffer[i] = b;

                //At every 4th byte consumed, get a new uint
                consumedCount++;
                if (consumedCount % 4 == 0)
                    next = (uint)Next();
                else
                    next = next >> 8;
            }
        }

        public double NextDouble()
        {
            return Next() * 2.328306e-10;
        }
    }

    public abstract class ByteRng : IRng
    {
        public abstract byte NextByte();

        public int Next()
        {
            uint a = NextByte(), b = NextByte(), c = NextByte(), d = NextByte();
            return (int) (a | b >> 8 | c >> 16 | d >> 24);
        }

        public int Next(int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            int next = Next();
            return (next.UMod(range)) + minValue;
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public virtual void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; ++i)
            {
                buffer[i] = NextByte();
            }
        }

        public double NextDouble()
        {
            return Next() * 2.328306e-10;
        }
    }

    public abstract class UInt64Rng : IRng
    {
        public abstract UInt64 NextLong();

        public int Next()
        {
            ulong next = NextLong();
            return (int) next ^ (int) (next >> 32);
        }

        public int Next(int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            int next = Next();
            return (next.UMod(range)) + minValue;
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public virtual void NextBytes(byte[] buffer)
        {
            //Cast to avoid sign extensions when shiftting
            ulong next = NextLong();
            //we could do Next(0,256) and that would discard 3/4 bytes
            byte consumedCount = 0;
            for (int i = 0; i < buffer.Length; ++i)
            {
                byte b = (byte)(next & 0xFF);
                buffer[i] = b;

                //At every 8th byte consumed, get a new ulong
                consumedCount++;
                if (consumedCount % 8 == 0)
                    next = NextLong();
                else
                    next = next >> 8;
            }
        }

        public double NextDouble()
        {
            return Next() * 2.328306e-10;
        }
    }
}
