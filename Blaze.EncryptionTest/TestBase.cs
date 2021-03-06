﻿using Blaze.Core.Extensions;
using Blaze.Core.Log;
using Blaze.Cryptography.Classics;
using Blaze.Cryptography.Extensions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Tests
{
    public class TestBase
    {
        protected ILogger Log = new TestLogger();

        protected List<EncryptTest> GetEncryptions()
        {
            var res = new ListConstruct<EncryptTest>
            {
                { new NullCypher(), "Null Cypher" },

                { new AutokeyCypher(), "AutoKey Cypher", TestType.All },

                { new BifidCypher(), "Bifid Cypher", TestType.Full | TestType.Xor },

                { new CaesarCypher(), "Caesar Cypher"},
                { new Vigenere(), "Vigenere Cypher"},

                { new HillCypher(), "Hill Cypher", TestType.All },

                { new Rot13(), "Rot 13", AlphabeticCypher.GetSimpleAlphabet(true, false, false), ROT13_TEXTS },

                { new ShuffleCypher(), "Shuffle Cypher", TestType.All },
                { new StreamCypher(), "Stream Cypher" },

                { new SaltedCypher(new StreamCypher()), "Salted Cypher", TestType.All },

                { new TranspositionCypher(), "Transposition Cypher" , TestType.All },
                { new ColumnarTranspositionCypher(), "Columnar Transposition Cypher" , TestType.All },

                { new FibonacciCypher(), "Fibonacci Cypher" },
                { new FibonacciCypherV2(), "Fibonacci Cypher V2" },
                { new FibonacciCypherV3(), "Fibonacci Cypher V3" },

                { new RandomBijection(new NullCypher()), "RBIJ on Null", TestType.NotXor},
                { new RandomBijection(new Vigenere()) , "RBIJ on Vigenere", TestType.NotXor},
                { new RandomBijection(new FibonacciCypher()) , "RBIJ on Fibonacci", TestType.NotXor},
                { new RandomBijection(new FibonacciCypherV3()), "RBIJ on FibonaccyV3" , TestType.NotXor },
                { new RandomBijection(new StreamCypher()), "RBIJ on Stream", TestType.NotXor},
                {
                    new ChainCypher(
                    typeof(FibonacciCypher),
                    typeof(StreamCypher),
                    typeof(FibonacciCypherV3)),
                    "Chain1",
                    TestType.Full
                },

            };
            return res;
        }

        public struct EncryptTest
        {
            public ICypher Enc;
            public string Name;
            public char[] Alpha;
            public TestType AllowedTypes;
            public string[] Keys;
            public string[] Texts;

            public EncryptTest(ICypher e, string name, char[] allowedAlpha, TestType types)
            {
                Enc = e;
                Name = name;
                Alpha = allowedAlpha;
                AllowedTypes = types;
                Keys = null;
                Texts = null;
            }

            public EncryptTest(ICypher e, string name) : this(e, name, null) { }

            public EncryptTest(ICypher e, string name, char[] allowedAlpha) : this(e, name, allowedAlpha, TestType.All) { }

            public EncryptTest(ICypher e, string name, TestType types) : this(e, name, null, types) { }

            public EncryptTest(ICypher e, string name, char[] allowedAlpha, string[] texts)
                : this(e, name, allowedAlpha)
            {
                Texts = texts;
            }
        }

        [Flags]
        public enum TestType
        {
            None = 0,
            // Full 256 alphabet
            Full = 1,
            // Xor is not revertible in modular arithmetic if its not a power of 2
            Xor = 2,
            //Used for restricted alphabet
            Alpha = 4,
            // we only use [a-z\sAHBWN] (26 + space + 5 caps chars)
            AlphaXor = 8,
            Custom = 16,
            All = 0b11111,
            Xors = Xor | AlphaXor,
            NotXor = Full | Alpha
        };

        
        protected TestResult TestEnc(EncryptTest test, TestType type)
        {
            return TestEnc(test, type, new List<string>(), new List<string>());
        }

        //Tests the test with the given type optionally giving back a summary and the
        // description of the failed tests
        protected TestResult TestEnc(EncryptTest test, TestType type, List<string> summary, List<string> failedTests)
        {
            Log.Info($"{test.Name} Test of type {type}");
            var texts = new List<string>();
            if (test.Alpha != null && type != TestType.Alpha)
            {
                Log.Warn($"{test.Name} has restricted alphabet, skipping general test\n\n\n");
                summary.Add($"Test for {test.Name} has been SKIPPED");
                return TestResult.Inconclusive | TestResult.Passed;//continue;
            }

            if (!test.AllowedTypes.HasFlag(type))
            {
                Log.Warn($"{test.Name} does not allow test of type {type}. Skipping.");
                summary.Add($"Test for {test.Name} has been SKIPPED");
                return TestResult.Inconclusive | TestResult.Passed;
            }

            if (test.Alpha != null)
            {
                Log.Info($"{test.Name} has restricted alphabet, testing with it.\n\n\n");
                Assert.IsTrue(type == TestType.Alpha);
            }

            texts = GetTexts(test, type);

            GetAlphabet(test, type);

            var keys = GetKeys(test, type);


            TestResult result;
            using (Log.StartIndentScope())
                result = TestEncTexts(test, type, keys, texts, failedTests);

            string res = string.Format("Test for {0} has {1}", test.Name, result);
            summary.Add(res);
            Log.Info(res);

            return result;
        }

        private TestResult TestEncTexts(EncryptTest test, TestType type, List<string> keys, List<string> texts, List<string> failedTests)
        {
            var enc = test.Enc;
            bool passInternal = true;
            Func<ICypher, string, string, bool> tester = GetTester(test, type);
            if (tester == null)
                return TestResult.Inconclusive; //cannot be tested in the current setting

            foreach (string k in keys)
            {
                Log.Info($"Testing with key '{k}'");
                foreach (var t in texts)
                {
                    bool currentPased = Utils.WrappedTest(() =>
                    {
                        using (Log.StartIndentScope())
                            return tester(enc, t, k);
                    }, Log);

                    if (!currentPased)
                        Log.Error($"Testing {test.Name} failed for key {k}");

                    passInternal &= currentPased;
                }
            }

            if (!passInternal)
                failedTests.Add(test.Name);

            return passInternal ? TestResult.Passed : TestResult.Failed;
        }

        protected virtual Func<ICypher, string, string, bool> GetTester(EncryptTest test, TestType type)
        {
            Func<ICypher, string, string, bool> tester = null;
            if (type == TestType.Full || type == TestType.Alpha)
            {
                tester = TestBackwardsForward;
            }
            else if (type == TestType.AlphaXor || type == TestType.Xor)
            {
                if ((test.AllowedTypes & TestType.Xors) == TestType.None)
                {
                    Log.Warn($"Skipping '{test.Name}'. Does not support XOR test.");
                    return null;
                }
                tester = TestBackwardsForwardXor;
            }
            else
                Assert.Fail("Unsupported test type!");
            return tester;
        }

        protected bool TestBackwardsForward(ICypher enc, string text, string key)
        {
            enc.ForwardOp = BasicOperations.Add.GetOpFunc();
            enc.ReverseOp = BasicOperations.Sub.GetOpFunc();
            string cypher = enc.Encrypt(text, key);
            string plain = enc.Decrypt(cypher, key);
            Log.Info($"Plain text: {text}");
            Log.Info($"Cypher text: {cypher}");
            Log.Info($"Decypher text: {plain}");
            Log.NewLine();
            return text == plain;
        }

        protected bool TestBackwardsForwardXor(ICypher enc, string text, string key)
        {
            enc.ForwardOp = BasicOperations.Xor.GetOpFunc();
            enc.ReverseOp = BasicOperations.Xor.GetOpFunc();
            string cypher = enc.Encrypt(text, key);
            string plain = enc.Decrypt(cypher, key);
            Log.Info($"Plain text: {text}");
            Log.Info($"Cypher text: {cypher}");
            Log.Info($"Decypher text: {plain}");
            Log.NewLine();
            return text == plain;
        }

        protected virtual List<string> GetTexts(EncryptTest test, TestType type)
        {
            var alpha = test.Alpha;
            var texts = new List<string>();
            if (type == TestType.Full || type == TestType.Xor)
            {
                //Anything goes
                texts.AddRange(new[] { "\0\0\0\0\0\0\0\0",
                                     "AAAAAA",
                                     "ABCDEFH" ,
                                     "Hello World!",
                                     "A Brave New World",
                                     "Potatoes are coool",
// Involution
@"An involution is a function {\displaystyle f:X\to X} f:X\to X that, when applied twice, brings one back to the starting point.
In mathematics, an involution, or an involutory function, is a function f that is its own inverse,
f(f(x)) = x
for all x in the domain of f.
",
// Common involutions
@"Any involution is a bijection.

The identity map is a trivial example of an involution. 

Common examples in mathematics of nontrivial involutions include multiplication by -1 in arithmetic, 
the taking of reciprocals, 
complementation in set theory and complex conjugation. 
Other examples include circle inversion, 
rotation by a half-turn, 
and reciprocal ciphers such as the ROT13 transformation and 
the Beaufort polyalphabetic cipher."
                });
            }
            else if (type == TestType.Alpha && alpha == null)
            {
                //No control characters
                texts.AddRange(new[] { "AAAAAA",
                                     "ABCDEFH" ,
                                     "Hello World",
                                     "A Brave New World",
                                     "Potatoes are coool" });
            }
            else if (type == TestType.AlphaXor)
            {
                //Only pow of 2 alphabets
                texts.AddRange(new[] { "AAAAAA",
                                     "abcdegh" ,
                                     "Hello World",
                                     "A Brave New World",
                                     "potatoes are coool" });
            }
            else if (type == TestType.Alpha && alpha != null)
            {
                texts.AddRange(new string[5]);
                //All first character
                texts[0] = Enumerable.Range(0, 10).Select(i => alpha[0]).Join();
                //All second character
                texts[1] = Enumerable.Range(0, 10).Select(i => alpha[1]).Join();
                //Flush
                texts[2] = Enumerable.Range(0, Math.Min(alpha.Length, 10)).Select(i => alpha[i]).Join();
                var r = new Random(0);
                //Random 10 character texts
                texts[3] = Enumerable.Range(0, 10).Select(i => alpha[r.Next(alpha.Length)]).Join();
                texts[4] = Enumerable.Range(0, 10).Select(i => alpha[r.Next(alpha.Length)]).Join();
            }
            else
                Assert.Fail("Trying to acquire an unknown's alphabet's text");

            if (test.Texts != null)
                texts.AddRange(test.Texts);

            return texts;
        }

        private static string[] ROT13_TEXTS =
        {
            "HELLOWORLD",
            "BYTHEPOWEROFTRUTHIWHILSTLIVINGHAVECONQUEREDTHEUNIVERSE",
            "POTATOESARECOOL",
            "FOLLOWTHEWHITERABIT"
        };

        protected virtual List<string> GetKeys(EncryptTest test, TestType type)
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

        protected virtual void GetAlphabet(EncryptTest test, TestType type)
        {
            var enc = test.Enc as AlphabeticCypher;
            if (type == TestType.Alpha)
            {
                if (test.Alpha == null)
                    enc.Alphabet = AlphabeticCypher.GetSimpleAlphabet();
                else //test with the actual alphabet of the encryption
                    enc.Alphabet = test.Alpha;
            }
            else if (type == TestType.AlphaXor)
                enc.Alphabet = AlphabeticCypher.GetPowerOf2Alphabet();
        }
    }
}
