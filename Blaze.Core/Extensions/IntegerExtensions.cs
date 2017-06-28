using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Extensions
{
    public static class IntegerExtensions
    {
        /// <summary>
        /// Unsigned modulo (e.g. -1 % 2 = 1 && -23 % 3 = 2)
        /// </summary>
        public static int UMod(this int self, int n)
        {
            return (self % n + n) % n;
        }


        private static int[] _Lookup;

        /// <summary>
        /// Counts the number of bits that are set on the byte.
        /// </summary>
        public static int CountBits(this byte self)
        {
            InitBitCountLookup();
            return _Lookup[self];
        }

        /// <summary>
        /// Counts the number of bits on the byte more slowly.
        /// Use this method if you only plan to do this once.
        /// </summary>
        public static int CountBitsSlow(this byte self)
        {
            int count = 0;
            while (self != 0)
            {
                count++;
                self &= (byte)(self - 1);
            }
            return count;
        }

        public static void InitBitCountLookup()
        {
            if (_Lookup == null)
            {
                _Lookup = new int[256];
                for (int ii = 0; ii < 256; ++ii)
                    _Lookup[ii] = ((byte)ii).CountBitsSlow();
            }
        }
    }
}
