using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Blaze.Cryptography.Rng.Marsaglia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography.Tests.Rng
{
    [TestClass]
    public class RangeTests
    {
        [TestMethod]
        public void SysRange()
        {
            Utils.ExecuteTester<SysRng>(TestRngRange);
        }

        [TestMethod]
        public void KissRange()
        {
            Utils.ExecuteTester<KissRng>(TestRngRange);
        }

        [TestMethod]
        public void MwcRange()
        {
            Utils.ExecuteTester<MultiplyWithCarryRng>(TestRngRange);
        }

        [TestMethod]
        public void LIB4Range()
        {
            Utils.ExecuteTester<LaggedFibonacciRng>(TestRngRange);
        }

        [TestMethod]
        public void SWBRange()
        {
            Utils.ExecuteTester<SubstractWithBorrowRng>(TestRngRange);
        }

        [TestMethod]
        public void SHR3Range()
        {
            Utils.ExecuteTester<ShiftRegisterRng>(TestRngRange);
        }

        [TestMethod]
        public void MarsagliaRange()
        {
            Utils.ExecuteTester<MSSRMRng>(TestRngRange);
        }

        [TestMethod]
        public void TriviumRange()
        {
            Utils.ExecuteTester<TriviumRng>(TestRngRange);
        }

        [TestMethod]
        public void CongRange()
        {
            Utils.ExecuteTester<CongruentialRng>(TestRngRange);
        }

        [TestMethod]
        public void NullRange()
        {
            Utils.ExecuteTester<NullRng>(TestRngRange);
        }

        private TestResult TestRngRange(Type rngType, out string errorMessage)
        {
            int[] seeds = { 0, 13, 23 };
            var ranges = new[]
            {
                //postive range
                new { Min = 13, Max = 51 },
                //Negative range
                new { Min = -51, Max = -13 },
                //Mixed range
                new { Min = -51, Max = 13 }
            };

            StringBuilder errBuilder = new StringBuilder();
            TestResult testRes = TestResult.Passed;

            foreach (int s in seeds)
            {
                IRng rng0 = (IRng)Activator.CreateInstance(rngType, s);

                foreach (var range in ranges)
                {
                    for (int i = 0; i < 2048; ++i)
                    {
                        int next = rng0.Next(range.Min, range.Max);
                        if (next < range.Min || next >= range.Max)
                        {
                            if (errBuilder.Length < 2048)
                                errBuilder.AppendLine($"Test failed for range [{range.Min}, {range.Max}[. Value was {next}");
                            testRes &= TestResult.Failed;
                        }
                    }
                }
            }
            errorMessage = errBuilder.ToString();

            return testRes;
        }
    }
}
