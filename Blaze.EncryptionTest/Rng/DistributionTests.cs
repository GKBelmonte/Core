using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blaze.Cryptography.Rng.Marsaglia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Cryptography.Rng;
using Blaze.Core.Log;
using Blaze.Core.Maths;

namespace Blaze.Cryptography.Tests.Rng
{
    [TestClass]
    public class DistributionTests
    {
        private ILogger Log = new TestLogger();
        [TestMethod]
        public void SysDistribution()
        {
            Utils.ExecuteTester<SysRng>(TestRngDistribution<SysRng>);
        }

        [TestMethod]
        public void KissDistribution()
        {
            Utils.ExecuteTester<KissRng>(TestRngDistribution<KissRng>);
        }

        [TestMethod]
        public void MwcDistribution()
        {
            Utils.ExecuteTester<MultiplyWithCarryRng>(TestRngDistribution<MultiplyWithCarryRng>);
        }

        [TestMethod]
        public void LIB4Distribution()
        {
            Utils.ExecuteTester<LaggedFibonacciRng>(TestRngDistribution<LaggedFibonacciRng>);
        }

        [TestMethod]
        public void SWBDistribution()
        {
            Utils.ExecuteTester<SubstractWithBorrowRng>(TestRngDistribution<SubstractWithBorrowRng>);
        }

        [TestMethod]
        public void SHR3Distribution()
        {
            Utils.ExecuteTester<ShiftRegisterRng>(TestRngDistribution<ShiftRegisterRng>);
        }

        [TestMethod]
        public void CongDistribution()
        {
            Utils.ExecuteTester<CongruentialRng>(TestRngDistribution<CongruentialRng>);
        }

        [TestMethod]
        public void MarsagliaDistribution()
        {
            Utils.ExecuteTester<MSSRMRng>(TestRngDistribution<MSSRMRng>);
        }

        [TestMethod]
        public void TriviumDistribution()
        {
            Utils.ExecuteTester<TriviumRng>(TestRngDistribution<TriviumRng>);
        }

        [TestMethod]
        public void NullDistribution()
        {
            Utils.ExecuteTester<NullRng>(TestRngDistribution<NullRng>);
        }

        [TestMethod]
        public void FlushDistribution()
        {
            Utils.ExecuteTester<FlushRng>(TestRngDistribution<FlushRng>);
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
            MarsagliaDistribution();
            TriviumDistribution();
        }

        private TestResult TestRngDistribution<T>(out string message) where T : IRng
        {
            message = string.Empty;
            TestResult testRes = TestResult.Passed;

            double score = RngTesting.DistributionTest<T>();

            Log.Info($"Distribution for {typeof(T).Name} scored {score.ToString("0.00000000")}");

            return testRes;
        }
    }
}
