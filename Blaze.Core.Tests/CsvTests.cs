using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Blaze.Core.Tests
{
    /// <summary>
    /// Summary description for CsvTests
    /// </summary>
    [TestClass]
    public class CsvTests
    {
        public CsvTests() { }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        class Alpha
        {
            public int One { get; set; }
            public List<string> Two {get; set;}
            public double Three { get; set; }
        }

        class Bravo
        {
            public string First { get; set; }
            public int[] Second { get; set; }
        }

        Alpha[] alphas =
        {
            new Alpha
            {
                One = 1,
                Two = new List<string> {"Echo","Bravo", ""},
                Three = 3.3
            },
            new Alpha
            {
                One = 2,
                Two = new List<string> {"Charlie", null},
                Three = 2.2
            },
        };

        Bravo[] bravos =
        {
            new Bravo
            {
                First = "A Name",
                Second = new[] {1, 2}
            },
            new Bravo
            {
                First = "A, B, C",
                Second = new[] {3, 4, 5}
            },
            new Bravo
            {
                First = "Talking Funny",
                Second = null
            },
        };

        class Alpha2 : Alpha
        {
            public Bravo Four { get; set; }
        }

        const string testCsvPath = @"E:\test\t.csv";

        [TestMethod]
        public void TestPrintSingleColumnCsv()
        {
            Csv csv = new Csv(testCsvPath);
            for (int i = 0; i < 9; ++i)
            {
                csv.SaveObject(i * i);
            }
            csv.Save();

            Csv loadCsv = new Csv(testCsvPath);
            csv.Load();

        }

        [TestMethod]
        public void TestPrintDoubleColumnCsv()
        {
            Csv csv = new Csv(testCsvPath);
            for (int i = 0; i < 9; ++i)
            {
                csv.SaveObject(i, collectionId: "x");
                csv.SaveObject(i * i, collectionId: "Squared");
            }
            csv.Save();
        }

        public class XYPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        [TestMethod]
        public void TestPrintSimpleObjectCsv()
        {
            Csv csv = new Csv(testCsvPath);
            for (int i = 0; i < 9; ++i)
            {
                csv.SaveObject(new XYPoint
                    {
                        X = i,
                        Y = i*i
                    }, 
                    collectionId: "x");
            }
            csv.Save();

            Csv loadCsv = new Csv(testCsvPath);
            csv.Load();
        }

        [TestMethod]
        public void TestPrintClassCsv()
        {
            Csv csv = new Csv(testCsvPath);
            csv.SaveObject(alphas[0], 0);
            csv.SaveObject(alphas[1], 1);
            csv.Save();

            Csv loadCsv = new Csv(testCsvPath);
            csv.Load();
        }

        [TestMethod]
        public void TestPrintTwoCollectionsCsv()
        {
            Csv csv = new Csv(testCsvPath);
            csv.SaveObject(alphas[0], 0);
            csv.SaveObject(alphas[1], 1);
            foreach (var b in bravos)
                csv.SaveObject(b, collectionId: "BravoCollection");
            csv.Save();

            Csv loadCsv = new Csv(testCsvPath);
            csv.Load();
        }

        [TestMethod]
        public void TestPrintPolyCsv()
        {
            Csv csv = new Csv(testCsvPath);
            var alphas2 = alphas.ToList();
            alphas2.AddRange(new[] 
            {
                new Alpha2(),
                new Alpha2() { Four = new Bravo() }
            });

            foreach (var a in alphas2)
                csv.SaveObject(a, collectionId: "Alpha");
            foreach (var b in bravos)
                csv.SaveObject(b, collectionId: "BravoCollection");
            csv.Save();
        }

        [TestMethod]
        public void TestSplitLine()
        {
            var testStrs = new[]
            {
                "a,b,c", //basic
                "a,,c", //one empty entry
                "\"One\",Two,Three", //One stringed entry
                "a,b,\"1,2,3\"", //stringed entry at the end
                "a,", //empty entry at end  a  null
                "\"\"\"\"", //One quote
                "\"\",\"\"", // two empty strings
                "\"\"\",\"\"\"", // ","
                "\"\"\"\",\"\"\",\"\"\",,", //  "   ","  null  null
                "one,\"\"\"\"\",,\"\"\"\"\""  //one  "",,""
            };

            var expectedSplits = new[]
            {
                new[] { "a", "b", "c"},
                new[] { "a", null, "c"},
                new[] { "One", "Two", "Three"},
                new[] { "a", "b", "1,2,3"},
                new[] { "a", null},
                new[] { "\"" },
                new[] { "", "" },
                new[] { "\",\"" },
                new[] { "\"", "\",\"", null, null},
                new[] { "one", "\"\",\"\""},
            };
            var splits = new List<List<string>>();
            var failedTests = new List<int>();
            for (int i = 0; i < testStrs.Length; ++i)
            { 
                string tst = testStrs[i];
                List<string> split = Csv.CsvSplitLine(tst);
                splits.Add(split);
                bool success = split.Count == expectedSplits[i].Length;

                for (int j = 0; j < split.Count && success; ++j)
                    success &= split[j] == expectedSplits[i][j];
            }
            Assert.IsTrue(
                !failedTests.Any(), 
                $"Tests {string.Join(",", failedTests)} failed");
        }

        [TestMethod]
        public void TestLoadCsv()
        {
            Csv csv = new Csv(testCsvPath);
            csv.Load();
        }
    }
}
