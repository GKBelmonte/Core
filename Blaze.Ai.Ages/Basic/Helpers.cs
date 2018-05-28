using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Ai.Ages.Basic
{
    public static class Helpers
    {
        public static float PolynomialEval(this CartesianIndividual individual , double[][] powsOfXforAllX, double[] expectedVals)
        {
            if (individual.Score.HasValue)
                return individual.Score.Value;

            double[] coefficients = individual.Values;

            Debug.Assert(expectedVals.Length == powsOfXforAllX.Length);
            double mse = 0;
            for (int i = 0; i < expectedVals.Length; ++i)
            {
                double[] powersOfX = powsOfXforAllX[i];
                double polynomialEval = 0;
                for (int j = 0; j < powersOfX.Length; ++j)
                {
                    polynomialEval += powersOfX[j] * coefficients[j];
                }

                double diff = (polynomialEval - expectedVals[i]);

                mse += diff * diff;
            }

            mse = mse / expectedVals.Length;

            individual.Score = (float)mse;

            return individual.Score.Value;
        }
    }
}
