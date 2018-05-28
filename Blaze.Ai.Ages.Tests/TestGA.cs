using Blaze.Ai.Ages.Basic;
using Blaze.Core.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Blaze.Ai.Ages.Tests
{
    //[TestFixture]
    [TestClass]
    public class TestGA
    {
        ILogger Log = new ConsoleLogger();

        //[Test]
        [TestMethod]
        public void Test_Squared()
        {
            Func<double, double> func = (x) => x * x;
            Test_PolynomialGA(func);
        }

        [TestMethod]
        public void Test_Cosine()
        {
            Func<double, double> func = (x) => Math.Cos(x);
            Test_PolynomialGA(func);
        }

        private void Test_PolynomialGA(Func<double, double> func)
        {
            int polynomialOrder = 5;
            int populationSize = 500;
            int sampleCount = 100;
            int genCount = 5000;

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

            var ages = new Ages(genCount,
                new Evaluate((i) => ((CartesianIndividual)i).PolynomialEval(allPowsOfX, expectedValues)),
                CartesianIndividual.CrossOver,
                new Generate(() => new CartesianIndividual(polynomialOrder)),
                pop);

            ages.GoThroughGenerationsSync();

            for (int i = 0; i < 4; ++i)
            {
                int ix = (ages.Champions.Count / 4) * i;
                Log.Info(ages.Champions[ix]);
            }
            Log.Info(ages.Champions.Last());
        }
    }
}
