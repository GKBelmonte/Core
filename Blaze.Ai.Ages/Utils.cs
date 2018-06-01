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
        public static double GausianNoise(this Random r, double s) 
        {
	        //5 random numbers from -1 to 1.
	        var d0 = r.NextDouble()*2.0 - 1.0;
	        var d1 = r.NextDouble()*2.0 - 1.0;
	        var d2 = r.NextDouble()*2.0 - 1.0;
	        var d3 = r.NextDouble()*2.0 - 1.0;
	        var d4 = r.NextDouble()*2.0 - 1.0;
	        return ((d0 + d1 + d2 + d3 + d4)*s);
        }

        public static double GausianNoise(double s)
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
