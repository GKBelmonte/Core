using Blaze.Core.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Tests
{
    public static class Utils
    {
        /// <summary>
        /// Will break as soon as a test fails and will
        /// repeat the test until you fix it in EnC
        /// (only in debugger)
        /// </summary>
        [DebuggerStepThrough]
        public static bool WrappedTest(Func<bool> test, ILogger log)
        {
            bool passed = true;
            do
            {
                if (Debugger.IsAttached && !passed)
                    Debugger.Break();
                try
                {
                    passed = test();
                }
                catch (Exception e)
                {
                    log.Error($"Internal exception: {e}");
                    passed = false;
                }

            } while (Debugger.IsAttached && !passed);
            return passed;
        }

        public static void TestResultAssert(this TestResult self, string message)
        {
            if (!self.HasFlag(TestResult.Passed))
                Assert.Fail(message);
            else if (self.HasFlag(TestResult.Inconclusive))
                Assert.Inconclusive(message);
        }

        public static void ExecuteTester<T>(this Tester tester, [CallerMemberName] string caller = null)
        {
            string message;
            tester(typeof(T), out message)
                .TestResultAssert($"Test {caller} failed. {message}");
        }
    }

    public delegate TestResult Tester(Type testedType, out string message);

    [Flags]
    public enum TestResult
    {
        Failed = 0x0,
        Passed =  0x1,
        Inconclusive = 0x10
    }
}
