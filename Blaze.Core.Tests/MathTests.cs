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
    }
}
