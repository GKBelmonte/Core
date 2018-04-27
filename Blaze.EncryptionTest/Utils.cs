using Blaze.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    }
}
