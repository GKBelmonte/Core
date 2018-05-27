using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Ai.Ages.Viewer
{
    public class GA
    {

        public GA()
        {
            Func<double, double> func = (x) => x*x;
            Test_PolynomialGA(func);
        }

        public Ages Ages { get; private set; }

        private void Test_PolynomialGA(Func<double, double> func)
        {
            int polynomialOrder = 5;
            int populationSize = 500;
            int sampleCount = 100;
            int genCount = 1;

            var pop = Enumerable
                .Range(0, populationSize)
                .Select(i => new PolynomialIndividual(polynomialOrder))
                .Cast<IIndividual>()
                .ToList();

            double[] xRange = Enumerable
                .Range(0, sampleCount)
                .Select(i => (double)i / 100)
                .ToArray();

            double[][] allPowsOfX = new double[sampleCount][];
            for (int i = 0; i < allPowsOfX.Length; ++i)
            {
                allPowsOfX[i] = new double[polynomialOrder];

                double powX = 1;
                for (int j = 0; j < allPowsOfX[i].Length; ++j)
                {
                    allPowsOfX[i][j] = powX;
                    powX *= xRange[i];
                }
            }

            double[] expectedValues = xRange
                .Select(i => func(i))
                .ToArray();

            Ages = new Ages(genCount,
                new Evaluate((i) => ((PolynomialIndividual)i).Eval(allPowsOfX, expectedValues)),
                PolynomialIndividual.CrossOver,
                new Generate((r) => new PolynomialIndividual(polynomialOrder)),
                pop);
        }

        public PolynomialIndividual Generation()
        {
            Ages.GoThroughGenerationsSync();
            var champ = (PolynomialIndividual)Ages.Champions.Last();
            return champ;
        }

        public class PolynomialIndividual : IIndividual
        {
            public double[] Factors { get; }
            public PolynomialIndividual() : this(6) { }

            public PolynomialIndividual(int order)
            {
                Name = IndividualTools.CreateName();
                Factors = new double[order];
                for (int i = 0; i < Factors.Length; ++i)
                {
                    Factors[i] = Utils.GausianNoise(5);
                }
            }

            public PolynomialIndividual(PolynomialIndividual other)
            {
                Name = IndividualTools.CreateName();
                Factors = new double[other.Factors.Length];
                for (int i = 0; i < Factors.Length; ++i)
                    Factors[i] = other.Factors[i];
            }

            public string Name { get; }

            public float MseScore { get; private set; }

            private void Normalize()
            {
                double min = Factors[0], max = Factors[0];
                for (int i = 1; i < Factors.Length; ++i)
                {
                    if (Factors[i] > max)
                        max = Factors[i];
                    if (Factors[i] < min)
                        min = Factors[i];
                }

                double diff = max - min;
                double epsilon = 0.00001;
                if (Math.Abs(diff - epsilon) < epsilon)
                {
                    diff = 1;
                }

                for (int i = 0; i < Factors.Length; ++i)
                {
                    double f = Factors[i];
                    Factors[i] = (f - min) / diff;
                }
            }

            public IIndividual Mutate(float probability, float sigma)
            {
                var newInd = new PolynomialIndividual(this);
                for (int i = 0; i < Factors.Length; ++i)
                {
                    if (Utils.ProbabilityPass(probability))
                        newInd.Factors[i] += Utils.GausianNoise(sigma);
                }
                return newInd;
            }

            public IIndividual Regenerate()
            {
                var ind = new PolynomialIndividual(Factors.Length);
                for (int i = 0; i < Factors.Length; ++i)
                {
                    ind.Factors[i] = Utils.GausianNoise(5);
                }
                return ind;
            }

            public static IIndividual CrossOver(List<IIndividual> parents)
            {
                var parent = (PolynomialIndividual)parents[0];
                var newInd = new PolynomialIndividual(parent.Factors.Length);
                for (var ii = 0; ii < newInd.Factors.Length; ++ii)
                {
                    //33% chance of taking single random parent gene, 66% chance of taking average of all parents
                    var dice = Utils.RandomInt(0, 3);
                    if (dice < 1)
                    {
                        //Take average of parents respective alleles
                        double tot = 0.0f;
                        for (var kk = 0; kk < parents.Count; ++kk)
                        {
                            var par = (PolynomialIndividual)parents[kk];
                            tot += par.Factors[kk];
                        }
                        tot = tot / parents.Count;
                        newInd.Factors[ii] = tot;
                    }
                    else //Take single parent gene randomly
                    {
                        var par = (PolynomialIndividual)parents[Utils.RandomInt(0, parents.Count)];
                        newInd.Factors[ii] = par.Factors[ii];
                    }
                }

                return newInd;
            }

            public float Eval(double[][] powsOfXforAllX, double[] expectedVals)
            {
                Debug.Assert(expectedVals.Length == powsOfXforAllX.Length);
                double mse = 0;
                for (int i = 0; i < expectedVals.Length; ++i)
                {
                    double[] powersOfX = powsOfXforAllX[i];
                    double polynomialEval = 0;
                    for (int j = 0; j < powersOfX.Length; ++j)
                    {
                        polynomialEval += powersOfX[j] * Factors[j];
                    }

                    double diff = (polynomialEval - expectedVals[i]);

                    mse += diff * diff;
                }

                mse = mse / expectedVals.Length;
                MseScore = (float)mse;
                return (float)mse;
            }

            public override string ToString()
            {
                return string.Join(",", Factors.Select(f => f.ToString("0.000")));
            }
        }

    }
}
