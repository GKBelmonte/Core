using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Blaze.Ai.Ages
{
    public static class Utils
    {
        public static int RandomInt (int p1, int p2)
        {
	        return ThreadRandom.Next(p1,p2);
        }

        /// <summary>
        /// Given a probability p, generates a random number, and
        /// checks if the probability passes as true or false 
        /// for one instance. 0 &lt;= p &lt; 1
        /// true if pass, false if fail
        /// </summary>
        public static bool ProbabilityPass(float p)
        {
            return ThreadRandom.ProbabilityPass(p);
        }

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

        //Given a standard deviation s, return a random value around 0 with that standard deviation
        public static double GausianNoise(this Random r, double s, int sampleCount = 7) 
        {
            //This factor is experimentally calculated for 
            // tens of thosands of samples.
            // It will most likely depend on the number 
            // of uniform samples taken
            const double correctionFactor = 2.0 / 3.0;
            //7 random numbers from -1 to 1.
            double accum = 0;
            for (int i = 0; i < sampleCount; ++i)
                accum += r.NextDouble() * 2.0 - 1.0;

            return (accum * s * correctionFactor);
        }

        public static double GaussianNoise(double s)
        {
            return ThreadRandom.GausianNoise(s);
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
