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

            Debug.Assert(expectedVals.Length == powsOfXforAllX.Length);

            double score = EvaluatePolynomial(individual, powsOfXforAllX)
                .Zip(expectedVals, (av, ev) =>
                {
                    double diff = av - ev;
                    return diff * diff;
                })
                .Sum();

            double mse = score / expectedVals.Length;

            individual.Score = (float)mse;

            return individual.Score.Value;
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
            if (individual.Score.HasValue)
                return individual.Score.Value;

            double sumOfSquares = individual.Values.Select(i => i * i).Sum();
            double sumOfCos = individual.Values.Select(i => Math.Cos(2 * Math.PI * i)).Sum();
            double res =  -20 * Math.Exp(-0.2 * Math.Sqrt(0.5 * sumOfSquares))
                - Math.Exp(0.5 * sumOfCos)
                + 20;
            individual.Score = (float)res;
            return individual.Score.Value;
        }
    }
}
