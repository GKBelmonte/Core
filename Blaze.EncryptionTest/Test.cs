using System;
using System.Collections.Generic;
using Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Blaze.Core.Extensions;
using System.Diagnostics;

namespace EncryptionTest
{
    [TestClass]
    public class Test
    {

        private List<EncryptTest> GetEncryptions()
        {
            var res = new ListConstruct<EncryptTest>
            {
                { new NullCypher(), "Null Cypher" },
                { new Rot13(), "Rot 13", AlphabeticEncrypt.GetSimpleAlphabet(true, false, false), ROT13_TEXTS },
                { new CaesarCypher(), "Caesar Cypher"},
                { new Vigenere(), "Vigenere Cypher"},
                { new StreamCypher(), "Stream Cypher" },
                { new FibonacciCypher(), "Fibonacci Cypher" },
                { new FibonacciCypherV2(), "Fibonacci Cypher V2" },
                { new FibonacciCypherV3(), "Fibonacci Cypher V3" },
                { new RandomBijection(new NullCypher()), "RBIJ on Null", TestType.NotXor},
                { new RandomBijection(new Vigenere()) , "RBIJ on Vigenere", TestType.NotXor},
                { new RandomBijection(new FibonacciCypher()) , "RBIJ on Fibonacci", TestType.NotXor},
                { new RandomBijection(new FibonacciCypherV3()), "RBIJ on FibonaccyV3" , TestType.NotXor },
                { new RandomBijection(new StreamCypher()), "RBIJ on Stream", TestType.NotXor}
                
            };
            return res;
        }

        public struct EncryptTest
        {
            public IOperationEncrypt Enc;
            public string Name;
            public char[] Alpha;
            public TestType AllowedTypes;
            public string[] Keys;
            public string[] Texts;

            public EncryptTest(IOperationEncrypt e, string name, char[] allowedAlpha, TestType types)
            {
                Enc = e;
                Name = name;
                Alpha = allowedAlpha;
                AllowedTypes = types;
                Keys = null;
                Texts = null;
            }

            public EncryptTest(IOperationEncrypt e, string name) : this(e, name, null) { }

            public EncryptTest(IOperationEncrypt e, string name, char[] allowedAlpha) : this(e, name, allowedAlpha, TestType.All) { }

            public EncryptTest(IOperationEncrypt e, string name, TestType types) : this(e, name, null, types) { }

            public EncryptTest(IOperationEncrypt e, string name, char[] allowedAlpha, string[] texts)
                : this(e, name, allowedAlpha)
            {
                Texts = texts;
            }
        }


        [Flags]
        public enum TestType
        {
            None = 0,
            Full = 1,
            Xor = 2,
            Alpha = 4,
            AlphaXor = 8,
            Custom = 16,
            All = 0x11111b,
            Xors = Xor | AlphaXor,
            NotXor = Full | Alpha
        };

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

        private static string[] ROT13_TEXTS =
        {
            "HELLOWORLD",
            "BYTHEPOWEROFTRUTHIWHILSTLIVINGHAVECONQUEREDTHEUNIVERSE",
            "POTATOESARECOOL",
            "FOLLOWTHEWHITERABIT"
        };

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
            using(new Log.IndentScope())
            {
                SimpleTest(typeof(FibonacciCypher), TestType.Full);
                SimpleTest(typeof(FibonacciCypher), TestType.Alpha);
                SimpleTest(typeof(FibonacciCypher), TestType.Xor);
                SimpleTest(typeof(FibonacciCypher), TestType.AlphaXor);
            }

            using (new Log.IndentScope())
            {
                SimpleTest(typeof(FibonacciCypherV2), TestType.Full);
                SimpleTest(typeof(FibonacciCypherV2), TestType.Alpha);
                SimpleTest(typeof(FibonacciCypherV2), TestType.Xor);
                SimpleTest(typeof(FibonacciCypherV2), TestType.AlphaXor);
            }

            using (new Log.IndentScope())
            {
                SimpleTest(typeof(FibonacciCypherV3), TestType.Full);
                SimpleTest(typeof(FibonacciCypherV3), TestType.Alpha);
                SimpleTest(typeof(FibonacciCypherV3), TestType.Xor);
                SimpleTest(typeof(FibonacciCypherV3), TestType.AlphaXor);
            }
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
                using (new Log.IndentScope())
                    pass &= TestEnc(test, type, summary, failedTests);
            }

            Log.Info("\n\n");
            summary.ForEach(s => Log.Info(s));

            Assert.IsTrue(pass, "One or more tests have failed: {0}", string.Join(", ", failedTests));
        }

        private bool TestEnc(EncryptTest test, TestType type)
        {
            return TestEnc(test, type, new List<string>(), new List<string>());
        }

        //Tests the test with the given type optionally giving back a summary and the
        // description of the failed tests
        private bool TestEnc(EncryptTest test, TestType type, List<string> summary, List<string> failedTests)
        {
            Log.Info("{0} Test of type {1}", test.Name, type);
            var texts = new List<string>();
            if (test.Alpha != null && type != TestType.Alpha)
            {
                Log.Info("{0} has restricted alphabet, skipping general test\n\n\n", test.Name);
                summary.Add(string.Format("Test for {0} has been SKIPPED", test.Name));
                return true;//continue;
            }
            if (test.Alpha != null)
            {
                Log.Info("{0} has restricted alphabet, testing with it.\n\n\n", test.Name);
                Assert.IsTrue(type == TestType.Alpha);
            }

            texts = GetTexts(test, type);

            GetAlphabet(test, type);
            
            var keys = GetKeys(test, type);


            bool passInternal;
            using(new Log.IndentScope())
                 passInternal = TestEncTexts(test, type, keys, texts, failedTests);

            string res = string.Format("Test for {0} has {1}", test.Name, passInternal ? "PASSED" : "!!!FAILED!!!");
            summary.Add(res);
            Log.Info(res);
            Log.Info("\n\n");

            return passInternal;
        }

        private static List<string> GetKeys(EncryptTest test, TestType type)
        {
            var res = new List<string>();
            switch (type)
            {
                case TestType.Full:
                case TestType.Xor:
                    res.Add("\0");
                    res.Add("KEY");
                    break;
                case TestType.AlphaXor: //we only use 32 chars for alpha-xor due to the pow2 limitation
                    res.Add(" "); //equiv 0
                    res.Add("key");
                    break;
                case TestType.Alpha:
                    if (test.Alpha == null)
                    {
                        res.Add("A"); //equiv 0
                        res.Add("KEY");
                    }
                    else
                    {
                        res.Add(test.Alpha[0].ToString()); //equiv 0
                        res.Add(Enumerable.Range(0, Math.Min(test.Alpha.Length, 3)).Select(i => test.Alpha[i]).Join());
                    }
                    break;
            }

            if (test.Keys != null)
                res.AddRange(test.Keys);

            return res;
        }

        private static void GetAlphabet(EncryptTest test, TestType type)
        {
            var enc = test.Enc as AlphabeticEncrypt;
            if (type == TestType.Alpha)
            {
                if(test.Alpha == null)
                    enc.Alphabet = AlphabeticEncrypt.GetSimpleAlphabet();
                else //test with the actual alphabet of the encryption
                    enc.Alphabet = test.Alpha;
            }
            else if (type == TestType.AlphaXor)
                enc.Alphabet = AlphabeticEncrypt.GetPowerOf2Alphabet();
        }

        private static List<string> GetTexts(EncryptTest test, TestType type)
        {
            var alpha = test.Alpha;
            var texts = new List<string>();
            if (type == TestType.Full || type == TestType.Xor)
            {
                texts.AddRange( new[] { "\0\0\0\0\0\0\0\0\0\0\0\0\0",
                                     "AAAAAA", 
                                     "ABCDEFH" ,
                                     "Hello World!", 
                                     "A Brave New World", 
                                     "Potatoes are coool" });
            }
            else if (type == TestType.Alpha && alpha == null)
            {
                texts.AddRange( new[] {"AAAAAA", 
                                     "ABCDEFH" ,
                                     "Hello World", 
                                     "A Brave New World", 
                                     "Potatoes are coool" });
            }
            else if (type == TestType.AlphaXor)
            {
                texts.AddRange(new[] {      "AAAAAA", 
                                     "abcdegh" ,
                                     "Hello World", 
                                     "A Brave New World", 
                                     "potatoes are coool" });
            }
            else if (type == TestType.Alpha && alpha != null)
            {
                texts.AddRange(new string[5]);
                texts[0] = Enumerable.Range(0, 10).Select(i => alpha[0]).Join();
                texts[1] = Enumerable.Range(0, 10).Select(i => alpha[1]).Join();
                texts[2] = Enumerable.Range(0, Math.Min(alpha.Length, 10)).Select(i => alpha[i]).Join();
                var r = new Random(0);
                texts[3] = Enumerable.Range(0, 10).Select(i => alpha[r.Next(alpha.Length)]).Join();
                texts[4] = Enumerable.Range(0, 10).Select(i => alpha[r.Next(alpha.Length)]).Join();
            }
            else
                Assert.Fail("Trying to acquire an unknown's alphabet's text");

            if (test.Texts != null)
                texts.AddRange(test.Texts);

            return texts;
        }

        private bool TestEncTexts(EncryptTest test, TestType type, List<string> keys, List<string> texts, List<string> failedTests)
        {
            var enc = test.Enc;
            bool passInternal = true;
            Func<IOperationEncrypt, string, string, bool> tester = GetTester(test, type);
            if (tester == null)
                return true; //cannot be tested in the current setting

            do
            {
                if (Debugger.IsAttached && !passInternal)
                    Debugger.Break();
                try
                {
                    foreach (string k in keys)
                    {
                        Log.Info("Testing with key '{0}'", k);
                        foreach (var t in texts)
                        {
                            using (new Log.IndentScope())
                            passInternal &= tester(enc, t, k);
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Info("Internal exception: {0}", e.ToString());
                    passInternal = false;
                }

            } while (Debugger.IsAttached && !passInternal);

            if (!passInternal)
                failedTests.Add(test.Name);
            
            return passInternal;
        }

        private Func<IOperationEncrypt, string, string, bool> GetTester(EncryptTest test, TestType type)
        {
            Func<IOperationEncrypt, string, string, bool> tester = null;
            if (type == TestType.Full || type == TestType.Alpha)
            {
                tester = TestBackwardsForward;
            }
            else if (type == TestType.AlphaXor || type == TestType.Xor)
            {
                if ((test.AllowedTypes & TestType.Xors) == TestType.None)
                {
                    Log.Info("Skipping '{0}'. Does not support XOR test.", test.Name);
                    return null;
                }
                tester = TestBackwardsForwardXor;
            }
            else
                Assert.Fail("Unsupported test type!");
            return tester;
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

        //private void SimpleTest(IOperationEncrypt enc, TestType testType, char[] alpha = null)
        //{

        //}

        private void SimpleTest(EncryptTest test, TestType testType, char[] alpha = null)
        {
            if (alpha != null)
                test.Alpha = alpha;
            bool pass = TestEnc(test, testType);
            Assert.IsTrue(pass);
        }

        private bool TestBackwardsForwardXor(IOperationEncrypt enc, string text, string key)
        {
            string cypher = enc.Encrypt(text, key, Operation.Xor);
            string plain = enc.Decrypt(cypher, key, Operation.Xor);
            Log.Info("Plain text: {0}", text);
            Log.Info("Cypher text: {0}", cypher);
            Log.Info("Decypher text: {0}", plain);
            Log.Info();
            return text == plain;
        }

        private bool TestBackwardsForward(IEncrypt enc, string text, string key)
        {
            string cypher = enc.Encrypt(text, key);
            string plain = enc.Decrypt(cypher, key);
            Log.Info("Plain text: {0}", text);
            Log.Info("Cypher text: {0}", cypher);
            Log.Info("Decypher text: {0}", plain);
            Log.Info();
            return text == plain;
        }
    
    
    }
}
