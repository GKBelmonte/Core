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
            //Func = (x) => x*x + x + 1;
            Func = (x) => Math.Sin(x);
            Test_PolynomialGA(Func);
        }

        public Ages Ages { get; private set; }

        Func<double, double> Func { get; }

        public double[] ExpectedValsX { get; private set; }
        public double[] ExpectedValsY { get; private set; }
        public double[] ActualValsY { get; private set; }
        public double[][] PowsOfXForAllX { get; private set; }

        public int PolynomialOrder { get; private set; }

        private void Test_PolynomialGA(Func<double, double> func)
        {
            Utils.SetRandomSeed(0);
            PolynomialOrder = 6;
            int populationSize = 150;
            int sampleCount = 1000;
            int genCount = 500;
            double step = 0.01;

            var pop = Enumerable
                .Range(0, populationSize)
                .Select(i => new CartesianIndividual(PolynomialOrder))
                .Cast<IIndividual>()
                .ToList();

            double[] xRange = Enumerable
                .Range(0, sampleCount)
                .Select(i => i * step)
                .ToArray();

            double[][] allPowsOfX = new double[sampleCount][];
            for (int i = 0; i < allPowsOfX.Length; ++i)
            {
                allPowsOfX[i] = new double[PolynomialOrder];

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

            ExpectedValsX = xRange;
            ExpectedValsY = expectedValues;
            PowsOfXForAllX = allPowsOfX;

            Ages = new Ages(genCount,
                new Evaluate((i) => ((CartesianIndividual)i).PolynomialEval(allPowsOfX, expectedValues)),
                //new Evaluate((i)=> (float)Helpers.EvaluateAckley(((CartesianIndividual)i))),
                CartesianIndividual.CrossOver,
                new Generate(() => new CartesianIndividual(PolynomialOrder)),
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
