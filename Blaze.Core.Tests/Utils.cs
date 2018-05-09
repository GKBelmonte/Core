using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Tests
{
    public static class Utils
    {
        public static void AreEqualWithEpsilon(
            this Assert self, 
            double expected, 
            double actual, 
            double epsilon = 0.00001)
        {
            double diff = System.Math.Abs(expected - actual);
            Assert.IsTrue(diff < epsilon);
        }
    }
}