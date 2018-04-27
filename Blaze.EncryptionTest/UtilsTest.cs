using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Core.Log;
using Blaze.Core.Extensions;

namespace Blaze.Cryptography.Tests
{
    /// <summary>
    /// Summary description for UtilsTest
    /// </summary>
    [TestClass]
    public class UtilsTest
    {
        ILogger Log = new TestLogger();
        public static string PlainText = @"using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Some.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod()
        {
        }
    }
}";


        [TestMethod]
        public void TestCheckPlainText()
        {
            var r = new Random(0);
            string plainEolWindows = PlainText;
            string plainEolUnix = PlainText.Replace("\r\n", "\n");
            string plainEolMac = PlainText.Replace("\r\n", "\r");
            string noEol = PlainText.Replace("\r\n", "");
            byte[] randBytes = new byte[256];
            r.NextBytes(randBytes);
            string randText = randBytes.ToTextString();


            var tests = new[]
            {
                new { Name = "Windows",     Text = plainEolWindows,   Plain = true, Eol = EndOfLine.Mixed },
                new { Name = "Unix",        Text = plainEolUnix,      Plain = true, Eol = EndOfLine.LineFeed },
                new { Name = "Mac",         Text = plainEolMac,       Plain = true, Eol = EndOfLine.CarriageReturn },
                new { Name = "None",        Text = noEol,             Plain = true, Eol = EndOfLine.None },
                new { Name = "Binary",      Text = randText,             Plain = false, Eol = EndOfLine.None },
            };

            bool success = true;
            var errs = new List<string>();
            foreach (var t in tests)
            {
                bool currentPased = Utils.WrappedTest(() =>
                {
                    using (Log.StartIndentScope())
                    {
                        EndOfLine eol;
                        bool isPlain = AlphabeticCypher.IsTextPlain(t.Text, out eol);
                        return isPlain == t.Plain && eol == t.Eol;
                    }
                }, Log);

                if (!currentPased)
                {
                    Log.Error($"Testing {t.Name} failed.");
                    errs.Add(t.Name);
                }
                success &= currentPased;
            }

            Assert.IsTrue(success, $"Following tests failed: {string.Join(",", errs)}");
        }
    }
}
