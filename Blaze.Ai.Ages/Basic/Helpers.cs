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
        /// <summary>
        /// Returns the mean-squared error of the individual if it's values
        /// represent the coefficients of a nth degree polynomial
        /// versus the expected values
        /// </summary>
        /// <param name="individual">The Cartesian individual made up of n+1 coefficients</param>
        /// <param name="powsOfXforAllX">First index represents the sample number (m). 
        /// Second index represents the order of the coefficient x[m][n]
        /// e.g. : x[1][2] = x_1^2 where x_1 is the 1st sample
        /// </param>
        /// <param name="expectedVals">The expected values for the sample veing evaluated at that point y[m]</param>
        /// <returns></returns>
        public static float PolynomialEval(this CartesianIndividual individual , double[][] powsOfXforAllX, double[] expectedVals)
        {
            Debug.Assert(expectedVals.Length == powsOfXforAllX.Length);

            double score = EvaluatePolynomial(individual, powsOfXforAllX)
                .Zip(expectedVals, (av, ev) =>
                {
                    double diff = av - ev;
                    return diff * diff;
                })
                .Sum();

            double mse = score / expectedVals.Length;

            if (double.IsPositiveInfinity(mse))
                Debugger.Break();

            return (float)Math.Log10(mse);
        }

        public static IEnumerable<double> EvaluatePolynomial(CartesianIndividual ind, double[][] powersOfXForAllX)
        {
            return powersOfXForAllX.Select(pows => _EvaluatePolynomial(ind, pows));
        }

        private static double _EvaluatePolynomial(CartesianIndividual ind, double[] powersOfX)
        {
            double[] coefficients = ind.Values;
            double polynomialEval = 0;
            for (int j = 0; j < coefficients.Length; ++j)
            {
                polynomialEval += powersOfX[j] * coefficients[j];
            }

            return polynomialEval;
        }

        public static double EvaluateAckley(CartesianIndividual individual)
        {
            double sumOfSquares = individual.Values.Select(i => i * i).Sum();
            double sumOfCos = individual.Values.Select(i => Math.Cos(2 * Math.PI * i)).Sum();
            double res =  -20 * Math.Exp(-0.2 * Math.Sqrt(0.5 * sumOfSquares))
                - Math.Exp(0.5 * sumOfCos)
                + 20;

            return (float)res;
        }
    }
}
