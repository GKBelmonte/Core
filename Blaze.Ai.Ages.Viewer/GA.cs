using Blaze.Ai.Ages.Basic;
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
            Utils.SetRandomSeed(0);
            int polynomialOrder = 5;
            int populationSize = 500;
            int sampleCount = 100;
            int genCount = 10;

            var pop = Enumerable
                .Range(0, populationSize)
                .Select(i => new CartesianIndividual(polynomialOrder))
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
                new Evaluate((i) => ((CartesianIndividual)i).PolynomialEval(allPowsOfX, expectedValues)),
                CartesianIndividual.CrossOver,
                new Generate(() => new CartesianIndividual(polynomialOrder)),
                pop);

            Ages.Distance = (l, r) => CartesianIndividual.Distance((CartesianIndividual)l, (CartesianIndividual)r);
        }

        public CartesianIndividual Generation()
        {
            Ages.GoThroughGenerationsSync();
            var champ = (CartesianIndividual)Ages.Champions.Last().Individual;
            return champ;
        }

    }
}
