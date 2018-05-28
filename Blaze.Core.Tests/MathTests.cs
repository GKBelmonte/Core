using System;
using System.Collections.Generic;
using Blaze.Core.Math;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blaze.Core.Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void TestChiSquared()
        {
            var occurrances = new List<int>
            {
                5,
                8,
                9,
                8,
                10,
                20
            };

            double cs = Stats.ChiSquared(occurrances);
            //wiki verification
            Assert.That.AreEqualWithEpsilon(13.4, cs);

            occurrances = new List<int>
            {
                10,
                10,
                10,
                10,
                10,
                10
            };

            cs = Stats.ChiSquared(occurrances);
            Assert.That.AreEqualWithEpsilon(0, cs);

            occurrances = new List<int>
            {
                60,
                0,
                0,
                0,
                0,
                0
            };

            //N(n - 1) where N is the observation size and
            // n is the number of cases
            // if each is equally likely (1/n expected frequency)

            cs = Stats.ChiSquared(occurrances);

            Assert.That.AreEqualWithEpsilon(300.0, cs);
        }

        [TestMethod]
        public void TestGTest()
        {
            //Uniform distribution
            var occurrances = new List<int>
            {
                10,
                10,
                10,
                10,
                10,
                10
            };

            double cs = Stats.GTest(occurrances);
            Assert.That.AreEqualWithEpsilon(0, cs);

            //Totally skewed distribution
            occurrances = new List<int>
            {
                60,
                0,
                0,
                0,
                0,
                0
            };

            cs = Stats.GTest(occurrances);
            const double run = 215.0111363;
            Assert.That.AreEqualWithEpsilon(run, cs, 0.001);

            //Ten times more occurrances
            occurrances = new List<int>
            {
                600,
                0,
                0,
                0,
                0,
                0
            };

            cs = Stats.GTest(occurrances);
            Assert.That.AreEqualWithEpsilon(run*10, cs, 0.001);

            // Evil Dice distribution
            occurrances = new List<int>
            {
                5,
                8,
                9,
                8,
                10,
                20
            };

            cs = Stats.GTest(occurrances);
            Assert.That.AreEqualWithEpsilon(11.757, cs, 0.001);
        }
    }
}
