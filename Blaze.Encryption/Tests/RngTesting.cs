using Blaze.Core.Math;
using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public static class RngTesting
    {
        /// <summary>
        /// Tests the Rng distribution 0 being a perfect score.
        /// The worst score is 1024* (256 - 10)
        /// </summary>
        /// <typeparam name="T">Rng type to test</typeparam>
        /// <returns></returns>
        public static double DistributionTest<T>() where T : IRng
        {
            // we get 1024 bytes, the expected frequency is 1/256
            // or 4 occurrances per byte.
            // Worst case, all ocurrances are the same byte
            // (no proof for that, but it works for 2 variables:
            // d(chi) = 2(f0 - e0)/e0 + 2(f1 - e1)/e1 = 0
            // f0 + f1 = e0 + e1 = 1, so  f0 + f1 = 1 are all zeroes)
            // In that worst case and uniform distribution, we have 
            // N(one case that happens all the time) + N(rest of the cases that never happen)  
            // N*[(1 - 1/n)^2/ (1/n)                   + (n - 1) (0-1/n)^2/(1/n)]
            //=N*n [(1 - 1/n)^2         +   (n - 1)/n^2]
            //=N*n[1 - 1/n] = N(n-1)
            //This is verified with the NullRng

            const int POP_SIZE= 1024;
            //double theoMax = POP_SIZE * (256 - 1);
            //random seeds from [0,65535]
            int[] seeds = { 0, 7501, 21584, 42875 };
            double[] seedResult = new double[seeds.Length];
            for (int j = 0; j < seeds.Length; ++j)
            {
                IRng rng = (IRng)Activator.CreateInstance(typeof(T), seeds[j]);
                float[] results = new float[16];

                byte[] bytes = new byte[POP_SIZE];

                for (int i = 0; i < 16; ++i)
                {
                    rng.NextBytes(bytes);
                    results[i] = (float)Stats.ChiSquared(EncryptionTesting.GetBlockCount(bytes));
                }

                seedResult[j] = results.Average();
            }

            double finalAverage = seedResult.Average();

            //if finalAverage = theoMax, we return 0
            // if finalAverage = 0, we return 1 (uniform distribution)
            //double percentResult = (theoMax - finalAverage) / theoMax;

            return finalAverage;
        }

        /// <summary>
        /// Tests the Rng confusion from 0 to 1.
        /// If the seed is changed by one bit, how much does the output change.
        /// Exactly 50%, gives a score of 1.
        /// 0% or 100% (!input) gives a score of 0
        /// </summary>
        public static float ConfusionTest<T>() where T : IRng
        {
            List<int> flipSeeds = CreateSeeds();
            var flipBytes = new List<byte[]>(16);
            List<int> seeds = CreateSeeds();

            List<IRng> rngs = seeds
                .Select(s => (IRng)Activator.CreateInstance(typeof(T), s))
                .ToList();

            foreach (IRng rng in rngs)
            {
                byte[] bytes = new byte[1024];
                rng.NextBytes(bytes);
                flipBytes.Add(bytes);
            }

            List<float> results = flipBytes
                .Select(b => EncryptionTesting.PercentFlips(flipBytes[0], b))
                .Skip(1)
                .ToList();

            float ave = results.Average();
            return (0.5f - Math.Abs(0.5f - ave)) / 0.5f;
        }

        public static List<int> CreateSeeds(int number = 16)
        {
            var rng = new Random(0);
            byte[] mainSeed = new byte[4];
            rng.NextBytes(mainSeed);
            List<byte[]> flips = EncryptionTesting.CreateFlips(mainSeed, number - 1);
            flips.Insert(0, mainSeed);

            return flips
                .Select(f => f.ToSeed())
                .ToList();
        }
    }
}
