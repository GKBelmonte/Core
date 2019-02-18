using Blaze.Ai.Ages.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Ai.Ages.Strats;

namespace Blaze.Ai.Ages.Viewer
{
    public class GA
    {
        public GA()
        {
            //Func = (x) => x*x + x + 1;
            Func = (x) => Math.Sin(x);
            //Func = (x) => 6*x + 3.3;
            //Func = (x) => 5 * x * x + 3 * x + 1;
            Test_PolynomialGA(Func);
        }

        public Ages Ages { get; private set; }

        Func<double, double> Func { get; }

        public double[] ExpectedValsX { get; private set; }
        public double[] ExpectedValsY { get; private set; }
        public double[] ActualValsY { get; private set; }
        public double[][] PowsOfXForAllX { get; private set; }

        public int PolynomialOrder { get; private set; }
        public int GenerationStop { get; private set; }

        private void Test_PolynomialGA(Func<double, double> func)
        {
            //Settings
            bool adaptive = false;
            int seed = 0;
            Utils.SetRandomSeed(seed);
            var rng = new Random(seed);
            PolynomialOrder = 5;
            int populationSize = 100;
            //number of samples to use the in the polynomial approx
            // to test the differnce
            double start = -10, end = 10;
            //Step between the samples
            double step = 0.02;
            //Number of generations to bundle
            int genBundleCount = 10;
            //Stop at Gen 
            GenerationStop = 500;

            //Get Data for tests
            double[] xRange, expectedValues;
            GetFofXForRange(func, start, step, end, out xRange, out expectedValues);

            double[][] allPowsOfX = GetPowersOfX(xRange, PolynomialOrder);

            ExpectedValsX = xRange;
            ExpectedValsY = expectedValues;
            PowsOfXForAllX = allPowsOfX;

            //Create Delegates
            Evaluate eval = (i) =>
                ((CartesianIndividual)i).PolynomialEval(allPowsOfX, expectedValues);

            CrossOver crossOver = adaptive
                ? AdaptiveCartesianIndividual.CrossOver
                : (CrossOver)CartesianIndividual.CrossOver;

            Generate generate = adaptive
                ? (Generate)((r) => new AdaptiveCartesianIndividual(PolynomialOrder, 1, r: r, emptySigma: true))
                : (r) => new CartesianIndividual(PolynomialOrder, 1, r);

            // Generate Population
            List<IIndividual> pop;
            pop = Enumerable
                .Range(0, populationSize)
                .Select(i => generate(rng))
                .Cast<IIndividual>()
                .ToList();

            Ages = new Ages(genBundleCount,
                eval,
                crossOver,
                generate,
                pop);

            Ages.SetRandomSeed(seed);

            Ages.NicheStrat = new NicheDensityStrategy(
                Ages,
                (l, r) => CartesianIndividual.Distance((CartesianIndividual)l, (CartesianIndividual)r));
        }

        private void GetFofXForRange(
            Func<double, double> func, 
            int sampleCount, 
            double step, 
            out double[] xRange, 
            out double[][] allPowsOfX, 
            out double[] expectedValues)
        {
            double offset = sampleCount * step / 2;

            xRange = Enumerable
                .Range(0, sampleCount)
                .Select(i => i * step - offset)
                .ToArray();
            allPowsOfX = new double[sampleCount][];
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

            expectedValues = xRange
                .Select(i => func(i))
                .ToArray();
        }

        private  static void GetFofXForRange(
            Func<double, double> func,
            double start,
            double step,
            double end,
            out double[] xRange,
            out double[] expectedValues)
        {
            int sampleCount = (int)((end - start) / step);

            xRange = Enumerable
                .Range(0, sampleCount)
                .Select(i => start + i * step)
                .ToArray();

            expectedValues = xRange
                .Select(i => func(i))
                .ToArray();
        }

        /// <summary>
        /// [10][2] represents x^2 for the 10th value of x
        /// </summary>
        private static double[][] GetPowersOfX(double[] xRange, int order)
        {
            int sampleCount = xRange.Length;
            double[][] allPowsOfX = new double[sampleCount][];
            for (int i = 0; i < allPowsOfX.Length; ++i)
            {
                allPowsOfX[i] = new double[order];

                double powX = 1;
                for (int j = 0; j < allPowsOfX[i].Length; ++j)
                {
                    allPowsOfX[i][j] = powX;
                    powX *= xRange[i];
                }
            }

            return allPowsOfX;
        }

        public Ages.EvaluatedIndividual Generation()
        {
            Ages.GoThroughGenerations();
            Ages.EvaluatedIndividual champ = Ages.Champions.Last();
            return champ;
        }

    }
}
