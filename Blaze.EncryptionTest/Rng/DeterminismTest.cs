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
    public class DeterminsmTests
    {
        [TestMethod]
        public void SysDeterminism()
        {
            Utils.ExecuteTester<SysRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void KissDeterminism()
        {
            Utils.ExecuteTester<KissRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void MwcDeterminism()
        {
            Utils.ExecuteTester<MultiplyWithCarryRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void LIB4Determinism()
        {
            Utils.ExecuteTester<LaggedFibonacciRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void SWBDeterminism()
        {
            Utils.ExecuteTester<SubstractWithBorrowRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void RC4Determinism()
        {
            Utils.ExecuteTester<RC4Rng>(TestRngDeterminism);
        }

        [TestMethod]
        public void SHR3Determinism()
        {
            Utils.ExecuteTester<ShiftRegisterRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void CongDeterminism()
        {
            Utils.ExecuteTester<CongruentialRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void MarsagliaDeterminism()
        {
            Utils.ExecuteTester<MSSRMRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void TriviumDeterminism()
        {
            Utils.ExecuteTester<TriviumRng>(TestRngDeterminism);
        }

        [TestMethod]
        public void NullDeterminism()
        {
            Utils.ExecuteTester<NullRng>(TestRngDeterminism);
        }

        private TestResult TestRngDeterminism(Type rngType, out string message)
        {
            message = string.Empty;
            int[] seeds = { 0, 13, 23 };

            TestResult testRes = TestResult.Passed;

            foreach (int s in seeds)
            {
                IRng rng0 = (IRng)Activator.CreateInstance(rngType, s);

                byte[] bytes0 = new byte[256];
                rng0.NextBytes(bytes0);

                IRng rng1 = (IRng)Activator.CreateInstance(rngType, s);

                byte[] bytes1 = new byte[256];
                rng1.NextBytes(bytes1);

                if (!bytes0.SequenceEqual(bytes1))
                    testRes &= TestResult.Failed;
            }
            return testRes;
        }
    }
}
