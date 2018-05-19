using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Rng
{
    internal static class RngHelpers
    {
        // stole this from https://referencesource.microsoft.com/#mscorlib/system/random.cs,92e3cf6e56571d5a,references, 
        // which they stole from Numerical Recipes in C (2nd Ed.)
        // so I guess its ok :D
        public static uint[] ArrayFromSeed(int seed)
        {
            const int MSEED = 161803398;
            int ii;
            uint mj, mk;
            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            int subtraction = (seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(seed);
            uint[] seedArray = new uint[56];
            mj = (uint)(MSEED - subtraction);
            seedArray[55] = mj;
            mk = 1;
            for (int i = 1; i < 55; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = (21 * i) % 55;
                seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += Int32.MaxValue;
                mj = seedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    seedArray[i] -= seedArray[1 + (i + 30) % 55];
                    if (seedArray[i] < 0) seedArray[i] += Int32.MaxValue;
                }
            }
            //The 3-4 least-significant nibles of this do not change if a high bit changes in the seeed
            return seedArray;
        }
    }
}
