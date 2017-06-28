using System;
using System.Collections.Generic;
using Encryption;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Core;
using System.Linq;
using System.Reflection;
using Blaze.Core.Extensions;

namespace EncryptionTest
{
    [TestClass]
    public class TestEnryptionTest
    {
        private static ListConstruct<TestCase> GetTestCases()
        {
            var tested = new ListConstruct<TestCase>
            {
                { new NullCypher(),"Null Cypher",  0f },
                {new Vigenere(), "Vigenere",  null },
                {new StreamCypher(), "Stream Cypher", null },
                {new FibonacciCypher(), "Fibonacci Cypher", null },
                {new FibonacciCypherV2(), "Fibonacci Cypher V2", null },
                {new FibonacciCypherV3(), "Fibonaccy Cypher V3",  null },
                {new CaesarCypher(), "Caesar Cypher", null},
                {new RandomBijection(new NullCypher()), "RandomBijectionDecorator(null)", 0f },
                {new RandomBijection(new CaesarCypher()), "RandomBijectionDecorator(CaesarCypher)", null },
                {new RandomBijection(new Vigenere()), "RandomBijectionDecorator(Vigenere)", null },
                {new RandomBijection(new FibonacciCypherV3()), "RandomBijectionDecorator(FibonacciCypherV3)", null },
                {new RandomBijection(new StreamCypher()), "RandomBijectionDecorator(StreamCypher)", null }
            };
            return tested;
        }

        [TestMethod]
        [TestCategory("Cypherness")]
        public void TestConfusionTest()
        {
            Console.WriteLine("Confusion: Measure of variance due to key");
            var tested = GetTestCases();
            EncryptionTesting.UseNatural = false;
            for (int b = 0; b < 2; b++)
            {
                Console.WriteLine("Using natural: {0}", EncryptionTesting.UseNatural);
                foreach (var test in tested)
                {
                    float res = EncryptionTesting.TestForConfusion(test.Enc, 10);
                    Console.WriteLine("{0} score for Confusion {1}", test.Name, res);
                    if (test.ExpectedVal != null)
                        Assert.IsTrue(Math.Abs(res - test.ExpectedVal.Value) < float.Epsilon, test.Name + " should score zero for confusion");
                }
                EncryptionTesting.UseNatural = true;
            }
        }

        [TestMethod]
        [TestCategory("Cypherness")]
        public void TestDiffusionTest()
        {
            // since we flip one bit in the plain text, the least diffusion
            // we can get is one bit changed. e.g. flips 1 / (1024*8 bits)
            // The score is matched against 50% bit flips. 0% means cypher == plain
            // and 100% cypher == !plain, so the best result is closest to random 50% 
            // Hence: (50% - |50% - flips%|) / 50%
            //const float EXPECTED_MIN = 0.000244140625f;
            Console.WriteLine("Confusion: Measure of variance due to a change in the plain text");
            float calc_min = 1f / (1024f * 8f);
            calc_min = (0.5f - Math.Abs(0.5f - calc_min)) / 0.5f;

            var tested = GetTestCases();
            foreach (var t in tested)
            {
                if (t.ExpectedVal.HasValue && t.ExpectedVal == 0)
                    t.ExpectedVal = calc_min;
            }

            EncryptionTesting.UseNatural = false;
            for (int b = 0; b < 2; b++)
            {
                Console.WriteLine("Using natural: {0}", EncryptionTesting.UseNatural);
                foreach (var test in tested)
                {
                    float res = EncryptionTesting.TestForDifussion(test.Enc, 10);
                    Console.WriteLine("{0} score for Diffusion {1}", test.Name, res);
                    if (test.ExpectedVal != null)
                        Assert.IsTrue(Math.Abs(res - test.ExpectedVal.Value) < 0.000001, test.Name + " should score zero for Diffusion");
                }
                EncryptionTesting.UseNatural = true;
            }
        }


        [TestMethod]
        [TestCategory("Cypherness")]
        public void TestDistributionTest()
        {
            Console.WriteLine("Distribution: Measure of uniform distribution");
            var tested = GetTestCases();
            EncryptionTesting.UseNatural = false;
            for (int b = 0; b < 2; b++)
            {
                Console.WriteLine("Using natural: {0}", EncryptionTesting.UseNatural);
                foreach (var test in tested)
                {
                    float res = EncryptionTesting.TestForDistribution(test.Enc);
                    Console.WriteLine("{0} score for distribution {1}", test.Name, res);
                    if (test.ExpectedVal != null)
                        Assert.IsTrue(Math.Abs(res - test.ExpectedVal.Value) < float.Epsilon, test.Name + " should score zero for distribution");
                }
                EncryptionTesting.UseNatural = true;
            }

        }
    }

    public class TestCase
    {
        public TestCase(IEncrypt b, string a, float? c)
        {
            Name = a;
            Enc = b;
            ExpectedVal = c;
        }
        public string Name { get; set; }
        public IEncrypt Enc { get; set; }
        public float? ExpectedVal { get; set; }
    }

    
}
