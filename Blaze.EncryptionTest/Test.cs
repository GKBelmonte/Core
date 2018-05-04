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
    public class Test : TestBase
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
        public void TranspositionCustomTest()
        {
            var cypher = new TranspositionCypher();
            cypher.Alphabet = AlphabeticCypher.GetSimpleAlphabet(true, false, false);
            string[] testStrings =
            {
                "WEMUSTATTACKATDAWN",
                "ATTACKATDAWN"
            };
            bool success = true;

            Log.Info("Without fill");
            using (Log.StartIndentScope()) 
            foreach (string testStr in testStrings)
            {
                Utils.WrappedTest(() =>
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
                Utils.WrappedTest(() =>
                {
                    string cypherStr = cypher.EncryptWithFill(testStr.ToByteArray(), key).ToTextString();
                    string plainStr = cypher.DecryptWithFill(cypherStr.ToByteArray(), key).ToTextString();
                    Log.Info($"Plain text: {testStr}");
                    Log.Info($"Cypher text: {cypherStr}");
                    Log.Info($"Decypher text: {plainStr}");
                    return plainStr == testStr;
                }, Log);
            }
        }

        [TestMethod]
        public void TranspositionColumnTest()
        {
            bool res = true;
            for (int next = 0; next < 10; ++next)
            {
                for (int pl = 0; pl < 1024; ++pl)
                {
                    Utils.WrappedTest(() =>
                    {
                        int cc = GetColCount(pl, next);
                        int cl = pl + (cc - pl % cc);
                        int newCc = GetReverseColCount(cl, next);
                        return newCc == cc;
                    }, Log);
                }
            }
        }
        private static int GetReverseColCount(int cypherLength, int next)
        {
            int diff = 0;
            int cc = 0;
            //Worst case plain length
            int tentativePlainLength = cypherLength;
            do
            {
                cc = GetColCount(tentativePlainLength, next);
                int tentativeCypherLength = tentativePlainLength + (cc - tentativePlainLength % cc);
                diff = cypherLength - tentativeCypherLength;
                tentativePlainLength--;

            } while (diff != 0);
            return cc;
        }

        private static int GetColCount(int plainLength, int next)
        {
            //+/-1/3*ave(sqrt(min),sqrt(max))
            int root = next;
            int columnCount = 0;
            if (plainLength <= 16) //4x4
                columnCount = ClampNext(root, 2, 4);
            else if (plainLength <= 64)  //8*8
                columnCount = ClampNext(root, 4, 8);
            else if (plainLength <= 256)  //16*16
                columnCount = ClampNext(root, 8, 16);
            else if (plainLength <= 1024)  //32*32
                columnCount = ClampNext(root, 16, 32);
            else if (plainLength <= 4096) //64*64
                columnCount = ClampNext(root, 32, 64);
            return columnCount;
        }

        private static int ClampNext(int next, int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            return (next.UMod(range)) + minValue;
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
                    pass &= TestEnc(test, type, summary, failedTests);
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
            bool pass = TestEnc(test, testType);
            Assert.IsTrue(pass, $"Testing {test.Name} failed for test '{testType}'");
        }
    }
}
