using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blaze.Cryptography.Rng.Marsaglia;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Cryptography.Rng;
using Blaze.Core.Log;

namespace Blaze.Cryptography.Tests.Rng
{
    [TestClass]
    public class ConfusionTests
    {
        private ILogger Log = new TestLogger();
        [TestMethod]
        public void SysConfusion()
        {
            Utils.ExecuteTester<SysRng>(TestRngConfusion<SysRng>);
        }

        [TestMethod]
        public void KissConfusion()
        {
            Utils.ExecuteTester<KissRng>(TestRngConfusion<KissRng>);
        }

        [TestMethod]
        public void MwcConfusion()
        {
            Utils.ExecuteTester<MultiplyWithCarryRng>(TestRngConfusion<MultiplyWithCarryRng>);
        }

        [TestMethod]
        public void LIB4Confusion()
        {
            Utils.ExecuteTester<LaggedFibonacciRng>(TestRngConfusion<LaggedFibonacciRng>);
        }

        [TestMethod]
        public void SWBConfusion()
        {
            Utils.ExecuteTester<SubstractWithBorrowRng>(TestRngConfusion<SubstractWithBorrowRng>);
        }

        [TestMethod]
        public void SHR3Confusion()
        {
            Utils.ExecuteTester<ShiftRegisterRng>(TestRngConfusion<ShiftRegisterRng>);
        }

        [TestMethod]
        public void CongConfusion()
        {
            Utils.ExecuteTester<CongruentialRng>(TestRngConfusion<CongruentialRng>);
        }

        [TestMethod]
        public void TriviumConfusion()
        {
            Utils.ExecuteTester<TriviumRng>(TestRngConfusion<TriviumRng>);
        }

        [TestMethod]
        public void NullConfusion()
        {
            Utils.ExecuteTester<NullRng>(TestRngConfusion<NullRng>);
        }

        [TestMethod]
        public void FlushConfusion()
        {
            Utils.ExecuteTester<IRng>(TestRngConfusion<FlushRng>);
        }

        [TestMethod]
        public void AllConfusion()
        {
            CongConfusion();
            KissConfusion();
            FlushConfusion();
            LIB4Confusion();
            MwcConfusion();
            NullConfusion();
            SHR3Confusion();
            SWBConfusion();
            SysConfusion();
            TriviumConfusion();
        }

        private TestResult TestRngConfusion<T>(out string message) where T : IRng
        {
            message = string.Empty;
            TestResult testRes = TestResult.Passed;

            float res = RngTesting.ConfusionTest<T>();

            Log.Info($"Confusion for {typeof(T).Name} scored {res.ToString("0.0000")}");

            return testRes;
        }
    }
}
