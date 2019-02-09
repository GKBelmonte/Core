using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;
using Blaze.Core.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Core.Maths;

namespace Blaze.Ai.Ages.Tests
{
    [TestClass]
    public class TestUtils
    {
        ILogger Log = new ConsoleLogger();

        [TestMethod]
        public void TestRadiusModifierFactor()
        {
            // If I want 3, 1, 0.333, the base has to be squared.
            // This is heuristically true, I just played with regressions until i found
            // the formula

            // f(n,p,m) = (1/m) * n  ^ (1 / Log(p, m*m))
            // Satisfies
            // f(p,p,m) = m
            // f(1,p,m) = 1/m
            // f(sqrt(p), p, m ) = 1
            // f(n,p,m) = (1/m) * n^(ln(m*m) / ln(p))

            double multiplier = 2;

            Func<double, double, double> radiusModifier = (nicheCount, popCount)
                => 1 / multiplier * Math.Pow(nicheCount, Math.Log(multiplier * multiplier) / Math.Log(popCount));

            double epsilon = 0.001;
            double[] expectedResults = {0.5, 1, 2};
            for (int popTest = 4; popTest <= 1024; ++popTest)
            {
                double[] nicheTests = { 1, Math.Sqrt(popTest), popTest };
                for (int i = 0; i < nicheTests.Length; ++i)
                {
                    double calculatedVal = radiusModifier(nicheTests[i], popTest);
                    Assert.IsTrue(Math.Abs(calculatedVal - expectedResults[i]) < epsilon);
                }
            }
        }

        [TestMethod]
        public void TestGaussianNoise()
        {
            const int sampleCount = 10000;// 00;
            double[] samples = new double[sampleCount];
            double[] sigmas = { 1, 2, 3, 5, 8, 13, 21, 34 };

            double devTolerance = 0.05; //5%
            double aveTolerance = 1; 

            double[] stdevs = new double[sigmas.Length];
            double[] aves = new double[sigmas.Length];
            for (int j = 0; j < sigmas.Length; ++j)
            {
                double sigma = sigmas[j];
                for (int i = 0; i < sampleCount;++i)
                {
                    samples[i] = Utils.GaussianNoise(sigmas[j]);
                }

                double ave = samples.Average();
                double stdDev = samples.StdDevS();
                double stdDevPop = samples.StdDevP();
                stdevs[j] = stdDevPop;
                aves[j] = ave;
            }
            var deviationError = sigmas
                .Zip(stdevs, (actual, test) => new
                {
                    Sigma = actual,
                    Test = test,
                    Err = Math.Abs(actual - test) / actual
                })
                .Where(err => err.Err > devTolerance)
                .Select(err => 
                    $"The error for stddev '{err.Sigma}' is too large '{err.Err * 100:0.}%'. " 
                    + $"Test: '{err.Test:0.}'. " 
                    + $"Tol: '{devTolerance * 100:0.}%'")
                .ToList(); ;

            var avereageErrors = aves
                .Where(av => Math.Abs(av) > aveTolerance)
                .Select(av => $"The average '{av}' is too large. ")
                .ToList();

            if (avereageErrors.Any() || deviationError.Any())
            {
                Assert.Fail(string.Join("\n", deviationError.Concat(avereageErrors)));
            }

        }
    }
}
