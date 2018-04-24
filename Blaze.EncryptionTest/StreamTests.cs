using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Extensions.Streams;

namespace Blaze.Cryptography.Tests
{
    /// <summary>
    /// Summary description for StreamTests
    /// </summary>
    [TestClass]
    public class StreamTests : TestBase
    {
        public StreamTests()
        {
            Log = new TestLogger(1);
        }

        [TestMethod]
        public void TestStream()
        {
            var tests = GetEncryptions();
            bool pass = true;
            List<string> summary = new List<string>(), failedTests = new List<string>(); 
            foreach (EncryptTest test in tests)
            {
                using (Log.StartIndentScope())
                    pass &= TestEnc(test, TestType.Full, summary, failedTests);
            }

            Log.Info("\n\n");
            summary.ForEach(s => Log.Info(s));

            Assert.IsTrue(pass, "One or more tests have failed: {0}", string.Join(", ", failedTests));
        }

        protected override Func<ICypher, string, string, bool> GetTester(EncryptTest test, TestType type)
        {
            return StreamTester;
        }

        private bool StreamTester(ICypher cypher, string plain, string key)
        {
            int capacity = plain.Length * 2 + 256; //2 bytes per char
            using (Stream plainStream = GenerateStreamFromString(plain))
            using (MemoryStream cryptoStream = new MemoryStream(capacity))
            using (MemoryStream backPlainStream = new MemoryStream(capacity))
            {

                byte[] keyArr = key.ToByteArray();

                plainStream.Position = 0;
                cryptoStream.Position = 0;
                backPlainStream.Position = 0;
                cypher.Encrypt(plainStream, keyArr, cryptoStream);

                plainStream.Position = 0;
                cryptoStream.Position = 0;
                backPlainStream.Position = 0;
                cypher.Decrypt(cryptoStream, keyArr, backPlainStream);

                plainStream.Position = 0;
                cryptoStream.Position = 0;
                backPlainStream.Position = 0;

                int origByte, newByte;
                do
                {
                    origByte = plainStream.ReadByte();
                    newByte = backPlainStream.ReadByte();
                    if (origByte != newByte)
                        return false;

                } while (origByte != -1 && newByte != -1);
                return true;
            }
        }

        public static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));
        }
    }
}
