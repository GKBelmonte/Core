using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blaze.Cryptography.Rng.Marsaglia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Cryptography.Rng;
using Blaze.Core.Log;
using Blaze.Core.Math;

namespace Blaze.Cryptography.Tests.Rng
{
    [TestClass]
    public class DistributionTests
    {
        private ILogger Log = new TestLogger();
        [TestMethod]
        public void SysDistribution()
        {
            Utils.ExecuteTester<SysRng>(TestRngConfusion<SysRng>);
        }

        [TestMethod]
        public void KissDistribution()
        {
            Utils.ExecuteTester<KissRng>(TestRngConfusion<KissRng>);
        }

        [TestMethod]
        public void MwcDistribution()
        {
            Utils.ExecuteTester<MultiplyWithCarryRng>(TestRngConfusion<MultiplyWithCarryRng>);
        }

        [TestMethod]
        public void LIB4Distribution()
        {
            Utils.ExecuteTester<LaggedFibonacciRng>(TestRngConfusion<LaggedFibonacciRng>);
        }

        [TestMethod]
        public void SWBDistribution()
        {
            Utils.ExecuteTester<SubstractWithBorrowRng>(TestRngConfusion<SubstractWithBorrowRng>);
        }

        [TestMethod]
        public void SHR3Distribution()
        {
            Utils.ExecuteTester<ShiftRegisterRng>(TestRngConfusion<ShiftRegisterRng>);
        }

        [TestMethod]
        public void CongDistribution()
        {
            Utils.ExecuteTester<CongruentialRng>(TestRngConfusion<CongruentialRng>);
        }

        [TestMethod]
        public void NullDistribution()
        {
            Utils.ExecuteTester<NullRng>(TestRngConfusion<NullRng>);
        }

        [TestMethod]
        public void FlushDistribution()
        {
            Utils.ExecuteTester<FlushRng>(TestRngConfusion<FlushRng>);
        }

        [TestMethod]
        public void AllDistribution()
        {
            CongDistribution();
            KissDistribution();
            FlushDistribution();
            LIB4Distribution();
            MwcDistribution();
            NullDistribution();
            SHR3Distribution();
            SWBDistribution();
            SysDistribution();
        }

        private TestResult TestRngConfusion<T>(out string message) where T : IRng
        {
            message = string.Empty;
            TestResult testRes = TestResult.Passed;

            double score = RngTesting.DistributionTest<T>();

            Log.Info($"Distribution for {typeof(T).Name} scored {score.ToString("0.00000000")}");

            return testRes;
        }
    }
}
