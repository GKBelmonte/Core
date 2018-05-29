using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Maths
{
    public static class Stats
    {
        public static double ChiSquared(IList<double> frequencies, int numberOfObs, IList<double> expectedFrequency = null )
        {
            // Number of cases normally denoted as n = ocurrances.Count
            int possibleOutcomes = frequencies.Count;
            //If null, assume uniform distribution
            if (expectedFrequency == null)
                expectedFrequency = frequencies.Select(f => 1.0 / possibleOutcomes).ToList();

            if (frequencies.Count != expectedFrequency.Count)
                throw new ArgumentException("Expected Frequencies should have the same count as frequencies");

            double accum = 0;
            for (int ii = 0; ii < frequencies.Count; ++ii)
            {
                double delta = (frequencies[ii] - expectedFrequency[ii]);
                accum += delta * delta / expectedFrequency[ii];
            }
            double chiSquared = accum * numberOfObs;
            return chiSquared;
        }

        /// <summary>
        /// Measure of distribution of the values.
        /// Zero would mean perfect (uniform) distribution
        /// </summary>
        public static double ChiSquared(IList<int> ocurrances, List<int> expectedOccurances = null)
        {
            //Find population size based on sum of occurances
            // Normally denoted as N
            int numberOfObs = ocurrances.Sum();
            // Number of cases normally denoted as n = ocurrances.Count

            //derive frequency
            IList<double> freq = ocurrances.Select(o => (double)o / numberOfObs).ToList();
            //if expected obs is given, calculate expected freq, else pass null.
            IList<double> expectedFreq = null;
            if (expectedOccurances != null)
                expectedFreq = expectedOccurances.Select(eo => (double)eo / numberOfObs).ToList();
            return ChiSquared(freq, numberOfObs, expectedFreq);
        }

        public static double GTest(IReadOnlyList<int> occurrances, IReadOnlyList<int> expectedOccurances = null)
        {
            // number of observations (normally N)
            int numberOfObservations = occurrances.Sum();
            // number of cases (normally n)
            int numberOfCases = occurrances.Count;

            //If null, assume uniform distribution 
            if (expectedOccurances == null)
                expectedOccurances = occurrances.Select(i => numberOfObservations/numberOfCases).ToList();

            if (occurrances.Count != expectedOccurances.Count)
                throw new ArgumentException($"{nameof(expectedOccurances)} should have the same count as {nameof(occurrances)}");

            double sum = occurrances
                .Zip(expectedOccurances,
                    (O_i, E_i) => O_i * System.Math.Log( (O_i == 0 ? 0.1 : O_i) / E_i) )
                .Sum();
            return 2 * sum;
        }

        private static double SquareDiffToAve(this IReadOnlyList<double> vals)
        {
            double ave = vals.Average();
            double squareDiff = vals.Select(v => v - ave)
                .Select(d => d * d)
                .Sum();

            return squareDiff;
        }

        public static double VarianceS(this IReadOnlyList<double> vals)
        {
            return vals.SquareDiffToAve() / (vals.Count - 1);
        }

        public static double VarianceP(this IReadOnlyList<double> vals)
        {
            return vals.SquareDiffToAve() / (vals.Count);
        }

        public static double StdDevS(this IReadOnlyList<double> vals)
        {
            return Math.Sqrt(vals.VarianceS());
        }

        public static double StdDevP(this IReadOnlyList<double> vals)
        {
            return Math.Sqrt(vals.VarianceP());
        }
    }
}
