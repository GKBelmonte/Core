using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Blaze.Core.Extensions;
using System.Diagnostics;
using Blaze.Core.Log;
using Blaze.Cryptography.Classics;

namespace Blaze.Cryptography.Tests
{
    [TestClass]
    public class EncryptDecryptTests : TestBase
    {
        [TestMethod]
        public void VigenereTestForthAndBack()
        {
            SimpleTest(typeof (Vigenere), TestType.Full);
        }

        [TestMethod]
        public void VigenereTestFixedAlpha()
        {
            SimpleTest(typeof(Vigenere), TestType.Alpha);
        }

        [TestMethod]
        public void VigenereTestXor()
        {
            SimpleTest(typeof(Vigenere), TestType.Xor);
            SimpleTest(typeof(Vigenere), TestType.AlphaXor);
        }

        [TestMethod]
        public void Rot13ForthAndBack()
        {
            SimpleTest(typeof(Rot13), TestType.Alpha);
        }

        [TestMethod]
        public void StreamTestForthAndBack()
        {
            SimpleTest(typeof(StreamCypher), TestType.Full);
        }

        [TestMethod]
        public void StreamTestFixedAlpha()
        {
            SimpleTest(typeof(StreamCypher), TestType.Alpha);
        }

        [TestMethod]
        public void StreamTestXor()
        {
            SimpleTest(typeof(StreamCypher), TestType.Xor);
            SimpleTest(typeof(StreamCypher), TestType.AlphaXor);
        }

        [TestMethod]
        public void Transposition()
        {
            SimpleTest(typeof(TranspositionCypher), TestType.Full);
        }

        [TestMethod]
        public void ColumnarTransposition()
        {
            SimpleTest(typeof(ColumnarTranspositionCypher), TestType.Full);
        }

        [TestMethod]
        public void TranspositionBasicTest()
        {
            var cypher = new TranspositionCypher();
            cypher.Alphabet = AlphabeticCypher.GetSimpleAlphabet(true, false, false);
            string[] testStrings =
            {
                "WEMUSTATTACKATDAWN",
                "ATTACKATDAWN",
                "TIMESUP"
            };
            bool success = true;

            Log.Info("Without fill");
            using (Log.StartIndentScope()) 
            foreach (string testStr in testStrings)
            {
                success &= Utils.WrappedTest(() =>
                {
                    string cypherStr = cypher.Encrypt(testStr.ToByteArray(), 4).ToTextString();
                    string plainStr = cypher.Decrypt(cypherStr.ToByteArray(), 4).ToTextString();
                    Log.Info($"Plain text: {testStr}");
                    Log.Info($"Cypher text: {cypherStr}");
                    Log.Info($"Decypher text: {plainStr}");
                    return plainStr == testStr;
                }, Log);
            }

            byte[] key = "KEY".ToByteArray();
            //With fill
            Log.Info("With fill");
            using (Log.StartIndentScope())
            foreach (string testStr in testStrings)
            {
                success &= Utils.WrappedTest(() =>
                {
                    string cypherStr = cypher.EncryptWithFill(testStr.ToByteArray(), key).ToTextString();
                    string plainStr = cypher.DecryptWithFill(cypherStr.ToByteArray(), key).ToTextString();
                    Log.Info($"Plain text: {testStr}");
                    Log.Info($"Cypher text: {cypherStr}");
                    Log.Info($"Decypher text: {plainStr}");
                    return plainStr == testStr;
                }, Log);
            }

            Assert.IsTrue(success, "One or more cases failed");
        }

        [TestMethod]
        public void ColumnarTranspositionCypherCustom()
        {
            var cypher = new ColumnarTranspositionCypher();
            cypher.Alphabet = AlphabeticCypher.GetSimpleAlphabet(true, false, false);
            string[] testStrings =
            {
                "TIMESUP",
                "WEMUSTATTACKATDAWN",
                "ATTACKATDAWN",
                "WEAREDISCOVEREDFLEEATONCE"
            };
            bool success = true;

            Log.Info("Without fill");
            string[] keys =
            {
                "DACB",
                "AKAB",
                "KEY",
                "ZEBRAS"
            };

            using (Log.StartIndentScope())
            {
                foreach (string keyStr in keys)
                {
                    byte[] key = keyStr.ToByteArray();
                    using (Log.StartIndentScope())
                    {
                        foreach (string testStr in testStrings)
                        {
                            success &= Utils.WrappedTest(() =>
                            {
                                byte[] testArr = testStr.ToByteArray();
                                byte[] cypherArr = cypher.EncryptClassic(testArr, key);
                                string plainStr = cypher.DecryptClassic(cypherArr, key)
                                    .ToTextString();
                                string cypherStr = cypherArr.ToTextString();
                                Log.Info($"Plain text: {testStr}");
                                Log.Info($"Cypher text: {cypherStr}");
                                Log.Info($"Decypher text: {plainStr}");
                                return plainStr == testStr;
                            }, Log);
                        }
                    }
                }
            }

            Assert.IsTrue(success, "One or more cases failed");
        }

        [TestMethod]
        public void RanBijectionCypherOnNullCypher()
        {
            SimpleTest("RBIJ on Null", TestType.Full);
        }

        [TestMethod]
        public void RanBijectionCypherOnNullKey()
        {
            SimpleTest("RBIJ on Vigenere", TestType.Full);
        }

        [TestMethod]
        public void RanBijectionCypherOnNullKeyAlpha()
        {
            SimpleTest("RBIJ on Vigenere", TestType.Alpha);
        }

        [TestMethod]
        public void RanBijectionCypherOnStream()
        {
            var test = new EncryptTest(new RandomBijection(new StreamCypher()), "RBIJ on Stream");
            SimpleTest(test, TestType.Full);
        }

        [TestMethod]
        public void FibonacciCypher()
        {
            Log.Info("Fibonacci Tests");
            using (Log.StartIndentScope())
            {
                SimpleTest(typeof(FibonacciCypher), TestType.Full);
                SimpleTest(typeof(FibonacciCypher), TestType.Alpha);
                SimpleTest(typeof(FibonacciCypher), TestType.Xor);
                SimpleTest(typeof(FibonacciCypher), TestType.AlphaXor);
            }

            using (Log.StartIndentScope())
            {
                SimpleTest(typeof(FibonacciCypherV2), TestType.Full);
                SimpleTest(typeof(FibonacciCypherV2), TestType.Alpha);
                SimpleTest(typeof(FibonacciCypherV2), TestType.Xor);
                SimpleTest(typeof(FibonacciCypherV2), TestType.AlphaXor);
            }

            using (Log.StartIndentScope())
            {
                SimpleTest(typeof(FibonacciCypherV3), TestType.Full);
                SimpleTest(typeof(FibonacciCypherV3), TestType.Alpha);
                SimpleTest(typeof(FibonacciCypherV3), TestType.Xor);
                SimpleTest(typeof(FibonacciCypherV3), TestType.AlphaXor);
            }
        }

        [TestMethod]
        public void AutoKeyCypherWikipedia()
        {
            // Check Wikipedia
            var tc = new Blaze.Cryptography.Classics.AutokeyCypher();
            tc.Alphabet = AlphabeticCypher.GetSimpleAlphabet(true, false, false);

            string k = "QUEENLY";
            string plain1 = "ATTACKATDAWN"; 
            string cypher1 = tc.Encrypt(plain1, k);
            Assert.AreEqual("QNXEPVYTWTWP", cypher1);
            string decypher1 = tc.Decrypt(cypher1, k);
            Assert.AreEqual(plain1, decypher1);

            string k2 = "KILT";
            string plain2 = "MEETATTHEFOUNTAIN";
            string cypher2 = tc.Encrypt(plain2, k2);
            Assert.AreEqual("WMPMMXXAEYHBRYOCA", cypher2);
            string decypher2 = tc.Decrypt(cypher2, k2);
            Assert.AreEqual(plain2, decypher2);
        }

        [TestMethod]
        public void AutoKeyCypher()
        {
            SimpleTest(typeof(AutokeyCypher), TestType.Full);
        }

        [TestMethod]
        public void BifidCypherWikipedia()
        {

            byte[] plainText = "FLEEATONCE".ToByteArray();
            byte[] polybiusSq = @"B G W K Z
                                 Q P N D S
                                 I O A X E
                                 F C L U M
                                 T H Y V R"
                .Replace(" ", string.Empty)
                .Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .ToByteArray();

            var bifid = new BifidCypher();
            var cypherBytes = bifid.EncryptClassic(plainText, polybiusSq);
            string cypher = cypherBytes.ToTextString();
            bool ok = cypher == "U  A  E  O  L  W  R  I  N  S".Replace(" ", string.Empty);

            string decypher = bifid.DecrpytClassic(cypherBytes, polybiusSq)
                .ToTextString();

            ok &= decypher == plainText.ToTextString();
            Assert.IsTrue(ok);
        }

        [TestMethod]
        public void BifidCypher()
        {
            SimpleTest(typeof(BifidCypher), TestType.Full);
        }

        [TestMethod]
        public void ShuffleCypherBasicTest()
        {
            var cypher = new ShuffleCypher();
            cypher.Alphabet = "0123456789".ToCharArray();
            string text = "0123";
            string cypherText = cypher.Encrypt(text, "A");
            string decypher = cypher.Decrypt(cypherText, "A");

            Assert.AreEqual(text, decypher);
        }

        [TestMethod]
        public void ShuffleCypher()
        {
            SimpleTest(typeof(ShuffleCypher), TestType.Full);
        }

        [TestMethod]
        public void SaltedCypherBasicTest()
        {
            var cypher = new SaltedCypher(new StreamCypher());
            cypher.Alphabet = "0123456789".ToCharArray();
            //Its not a pow 2 alphabet, so xor does not work
            cypher.ForwardOp = (a, b) => a + b;
            cypher.ReverseOp = (a, b) => a - b;
            string text = "0123";
            string cypherText = cypher.Encrypt(text, "K");
            string decypherText = cypher.Decrypt(cypherText, "K");
            Log.Info($"Plain Text: {text}");
            Log.Info($"Cypher Text: {cypherText}");
            Log.Info($"Decypher Text: {decypherText}");
            Assert.AreEqual(text, decypherText);
        }

        [TestMethod]
        public void SaltedCypherTest()
        {
            SimpleTest(typeof(SaltedCypher), TestType.Full);
        }

        [TestMethod]
        public void HillCypherBasicTest()
        {
            var hill = new HillCypher();
            hill.Alphabet = "0123456789".ToCharArray();
            string text = "0123";
            string cypherText = hill.Encrypt(text, "K");
            string decypherText = hill.Decrypt(cypherText, "K");
            Log.Info($"Plain Text: {text}");
            Log.Info($"Cypher Text: {cypherText}");
            Log.Info($"Decypher Text: {decypherText}");
            Assert.AreEqual(text, decypherText);
        }

        [TestMethod]
        public void HillCypherTest()
        {
            SimpleTest(typeof(HillCypher), TestType.Full);
        }
        
        [TestMethod]
        public void BasicMazeCypher()
        {
            var c = new MazeCypher();

            var testInfos = new[]
            {
                new {Str = "HELLO, WORLD", Col = 4 } ,
                new {Str = "A MAZE IS A COLLECTION OF PATHS, TYPICALLY FROM A START TO N END", Col = 8 } ,
            };
            foreach (var testInfo in testInfos)
            {
                string plainStr = testInfo.Str;
                byte[] plainBytes = plainStr.ToByteArray();
                var keystream = new Blaze.Cryptography.Rng.Marsaglia.MSSRMRng(8);
                Maze m;
                byte[] cypherBytes = c.Encrypt(plainBytes, testInfo.Col, keystream, out m);
                string cypherStr = cypherBytes.ToTextString();
                Log.Info(cypherStr);
                Log.Info(m);
                keystream = new Blaze.Cryptography.Rng.Marsaglia.MSSRMRng(8);
                byte[] decypher = c.Decrypt(cypherBytes, testInfo.Col, keystream);
                string decypherStr = decypher.ToTextString();

                Assert.AreEqual(plainStr, decypherStr);
            }
        }

        [TestMethod]
        public void NullCypher()
        {
            SimpleTest(typeof(NullCypher), TestType.Full);
        }

        [TestMethod]
        public void AllCyphersForwardBackwards()
        {
            AllTest(TestType.Full);
        }

        [TestMethod]
        public void AllCyphersForwardBackwardsAlpha()
        {
           AllTest(TestType.Alpha);
        }

        [TestMethod]
        public void AllCyphersForwardBackwardsXor()
        {
            AllTest(TestType.Xor);
        }

        [TestMethod]
        public void AllCyphersForwardBackwardsAlphaXor()
        {
            AllTest(TestType.AlphaXor);
        }

        private void AllTest(TestType type)
        {
            var encs = GetEncryptions();
            bool pass = true;
            var summary = new List<string>(encs.Count);
            var failedTests = new List<string>();
            foreach (EncryptTest test in encs)
            {
                using (Log.StartIndentScope())
                {
                    // Ignore inconclusive for all test
                    pass &= TestEnc(test, type, summary, failedTests)
                        .HasFlag(TestResult.Passed);
                }
            }

            Log.Info("\n\n");
            summary.ForEach(s => Log.Info(s));

            Assert.IsTrue(pass, "One or more tests have failed: {0}", string.Join(", ", failedTests));
        }

        private void SimpleTest(Type encType, TestType testType, char[] alpha = null)
        {
            var test = GetEncryptions().FirstOrDefault(t => t.Enc.GetType() == encType);
            SimpleTest(test, testType, alpha);
        }

        private void SimpleTest(string name, TestType testType, char[] alpha = null)
        {
            var test = GetEncryptions().FirstOrDefault(t => t.Name == name);
            SimpleTest(test, testType, alpha);
        }

        private void SimpleTest(EncryptTest test, TestType testType, char[] alpha = null)
        {
            if (alpha != null)
                test.Alpha = alpha;
            TestResult pass = TestEnc(test, testType);
            pass.TestResultAssert($"Testing {test.Name} failed for test '{testType}'");
        }
    }
}
