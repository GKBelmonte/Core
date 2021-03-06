﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blaze.Core;
using System.Linq;
using System.Reflection;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Classics;
using Blaze.Core.Log;

namespace Blaze.Cryptography.Tests
{
    [TestClass]
    public class TestEnryptionTest
    {
        ILogger Log = new TestLogger();
        public static ListConstruct<TestCase> GetTestCases()
        {
            var testedCyphers = new ListConstruct<TestCase>
            {
                { new NullCypher(),         "Null Cypher",  0f },
                { new Vigenere(),           "Vigenere",  null },
                { new StreamCypher(),       "Stream Cypher", null },
                { new FibonacciCypher(),    "Fibonacci Cypher", null },
                { new FibonacciCypherV2(),  "Fibonacci Cypher V2", null },
                { new FibonacciCypherV3(),  "Fibonaccy Cypher V3",  null },
                { new CaesarCypher(),       "Caesar Cypher", null},
                { new RandomBijection(new NullCypher()),        "RandomBijectionDecorator(null)", null },
                { new RandomBijection(new CaesarCypher()),      "RandomBijectionDecorator(CaesarCypher)", null },
                { new RandomBijection(new Vigenere()),          "RandomBijectionDecorator(Vigenere)", null },
                { new RandomBijection(new FibonacciCypherV3()), "RandomBijectionDecorator(FibonacciCypherV3)", null },
                { new RandomBijection(new StreamCypher()),      "RandomBijectionDecorator(StreamCypher)", null },
                {
                    new ChainCypher(
                    typeof(FibonacciCypher),
                    typeof(StreamCypher),
                    typeof(FibonacciCypherV3)),
                    "Chain1",
                    null
                },
                { new TranspositionCypher(), "Transposition Cypher", null }
            };
            return testedCyphers;
        }

        [TestMethod]
        [TestCategory("Cypherness")]
        public void TestConfusionTest()
        {
            Log.Info("Confusion: Measure of variance due to key");
            var tested = GetTestCases();

            using (Log.StartIndentScope())
            foreach (EncryptionTesting.TextType val in Enum.GetValues(typeof(EncryptionTesting.TextType)))
            {
                EncryptionTesting.Type = val;
                Log.Info($"Text Type: {EncryptionTesting.Type}");

                using (Log.StartIndentScope())
                foreach (var test in tested)
                {
                    float res = EncryptionTesting.TestForConfusion(test.Cypher, 10);
                    Log.Info($"{test.Name} score for Confusion {res}");
                    if (test.ExpectedVal != null)
                        Assert.IsTrue(Math.Abs(res - test.ExpectedVal.Value) < float.Epsilon, test.Name + " should score zero for confusion");
                }
            }
        }

        [TestMethod]
        [TestCategory("Cypherness")]
        public void TestDiffusionTest()
        {
            // since we flip one bit in the plain text, the least diffusion
            // we can get is one bit changed. e.g. flips 1 / (1024*8 bits) 
            // (e.g.: 1 flip / plain-length in bits)
            // The score is matched against 50% bit flips. 0% means cypher == plain
            // and 100% cypher == !plain, so the best result is closest to random 50% 
            // Hence: (50% - |50% - flips%|) / 50%
            //const float EXPECTED_MIN = 0.000244140625f;
            Log.Info("Diffusion: Measure of variance due to a change in the plain text");
            float calc_min = 1f / (1024f * 8f);
            calc_min = (0.5f - Math.Abs(0.5f - calc_min)) / 0.5f;

            var tested = GetTestCases();
            foreach (var t in tested)
            {
                if (t.ExpectedVal.HasValue && t.ExpectedVal == 0)
                    t.ExpectedVal = calc_min;
            }

            using (Log.StartIndentScope())
            for (int b = 0; b < 4; b++)
            {
                EncryptionTesting.Type = (EncryptionTesting.TextType)b;
                Log.Info($"Text Type: {EncryptionTesting.Type}");

                using (Log.StartIndentScope())
                foreach (var test in tested)
                {
                    float res = EncryptionTesting.TestForDifussion(test.Cypher, 10);
                    Log.Info($"{test.Name} score for Diffusion {res}");
                    if (test.ExpectedVal != null)
                        Assert.IsTrue(Math.Abs(res - test.ExpectedVal.Value) < 0.000001, test.Name + " should score zero for Diffusion");
                }
            }
        }


        [TestMethod]
        [TestCategory("Cypherness")]
        public void TestDistributionTest()
        {
            Log.Info("Distribution: Measure of uniform distribution");
            var tested = GetTestCases();

            using (Log.StartIndentScope())
            for (int b = 0; b < 4; b++)
            {
                EncryptionTesting.Type = (EncryptionTesting.TextType)b;
                Log.Info($"Text Type: {EncryptionTesting.Type}");

                using (Log.StartIndentScope())
                foreach (var test in tested)
                {
                    float res = EncryptionTesting.TestForDistribution(test.Cypher);
                    Log.Info($"{test.Name} score for distribution {res}");
                    if (test.ExpectedVal != null)
                        Assert.IsTrue(Math.Abs(res - test.ExpectedVal.Value) < float.Epsilon, test.Name + " should score zero for distribution");
                }
            }

        }
    }


}
