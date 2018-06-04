using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Core.Log;
using Blaze.Core.Extensions;
using System.Linq;

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

        [TestMethod]
        public void TestEncodeBytes()
        {
            byte[] test = { 0x01, 0xFF, 0x07, 100 };
            var encoded = CryptoUtils.EncodeBytes(test, 10).ToList();
            int[] expected = 
            {
                4, 0,
                1, 0, 0,
                5, 5, 2,
                7, 0, 0,
                0, 0, 1
            };
            Assert.AreEqual(expected.Length, encoded.Count);
            for (int i = 0; i < expected.Length; ++i)
                Assert.AreEqual(expected[i], encoded[i]);
        }

        [TestMethod]
        public void TestDecodeBytes()
        {
            int[] test =
            {
                4, 0,
                1, 0, 0,
                5, 5, 2,
                7, 0, 0,
                0, 0, 1
            };
            byte[] expected = { 0x01, 0xFF, 0x07, 100 };
            int skipCount;
            var decoded = CryptoUtils.DecodeBytes(test, 10, out skipCount).ToList();

            Assert.AreEqual(expected.Length, decoded.Count);
            for (int i = 0; i < expected.Length; ++i)
                Assert.AreEqual(expected[i], decoded[i]);
            Assert.AreEqual(test.Length, skipCount);
        }
    }
}
