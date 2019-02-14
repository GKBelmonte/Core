using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Blaze.Ai.Ages
{
    public static class Utils
    {
        private static double[] _GaussianNoiseCorrection;
        static Utils()
        {
            _GaussianNoiseCorrection = new double[8];
            for (int i = 1; i < _GaussianNoiseCorrection.Length; ++i)
                _GaussianNoiseCorrection[i] = 7.0 / (4.0 * Math.Sqrt(i));
        }

        public static bool ProbabilityPass(float p)
        {
            return ThreadRandom.ProbabilityPass(p);
        }

        /// <summary>
        /// Given a probability p, generates a random number, and
        /// checks if the probability passes as true or false 
        /// for one instance. 0 &lt;= p &lt; 1
        /// true if pass, false if fail
        /// </summary>
        public static bool ProbabilityPass(this Random r, float p)
        {
	        var dice = r.NextDouble();
	        if( dice < p )
		        return true;
	        else
		        return false;
        }

        private static ThreadLocal<Random> rand = new ThreadLocal<Random>(() => new Random());

        public static Random ThreadRandom { get { return rand.Value; } }

        /// <summary>
        /// Given a standard deviation s, return a random value around 0 with that standard deviation
        /// normally distributed.
        /// </summary>
        /// <param name="s">The standard deviation for the random number</param>
        /// <param name="sampleCount">The number of samples between 1 and 7 inclusive.
        /// The more samples, the better the normal distribution, but the more expensive.
        /// Using one sample will result in an uniform distribution. </param>
        public static double GausianNoise(this Random r, double s, int sampleCount = 4) 
        {
            if (sampleCount < 1 || sampleCount > 7)
                throw new ArgumentOutOfRangeException(
                    nameof(sampleCount), 
                    "Value should be betwee 1 and 7 inclusive");
            //This factor is experimentally calculated for 
            // tens of thosands of samples.
            // It will most likely depend on the number 
            // of uniform samples taken
            /// Samples  Correction
            /// 1        1/0.57         7/4
            /// 3        1              1/1
            /// 7        1/1.5          2/3
            /// ... heuristically I get 7/ (4*srt(sc)) where sc is sample count

            double correctionFactor = _GaussianNoiseCorrection[sampleCount];
            //7 random numbers from -1 to 1.
            double accum = 0;
            for (int i = 0; i < sampleCount; ++i)
                accum += r.NextDouble() * 2.0 - 1.0;

            return (accum * s * correctionFactor);
        }

        public static double GaussianNoise(double s, int sampleCount = 4)
        {
            return ThreadRandom.GausianNoise(s, sampleCount);
        }

        public static void SetRandomSeed(int seed)
        {
            rand.Value = new Random(seed);
        }

        internal static Ages.EvaluatedIndividual ToEI(this IIndividual individual)
        {
            return new Ages.EvaluatedIndividual { Individual = individual };
        }
    }
}
