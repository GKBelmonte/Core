using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Math
{
    public static class Stats
    {
        public static double ChiSquared(IList<double> frequencies, int numberOfObs, IList<double> expectedFrequency = null )
        {
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

        public static double ChiSquared(IList<int> ocurrances, List<int> expectedOccurances = null)
        {
            //Find population size based on sum of occurances
            int numberOfObs = ocurrances.Sum();
            //derive frequency
            IList<double> freq = ocurrances.Select(o => (double)o / numberOfObs).ToList();
            //if expected obs is given, calculate expected freq, else pass null.
            IList<double> expectedFreq = null;
            if (expectedOccurances != null)
                expectedFreq = expectedOccurances.Select(eo => (double)eo / numberOfObs).ToList();
            return ChiSquared(freq, numberOfObs, expectedFreq);
        }
    }
}
