using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections;

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

        const string testCsvPath = @"E:\test\t.csv";

        class TestCollection<T> : List<T>, ITestCollection
        {
            public string ID { get; set; }
            public Type Type { get; set; }
            public TestCollection() { Type = typeof(T); }
        }

        interface ITestCollection : IList
        {
            string ID { get; set; }
            Type Type { get; set; }
        }

        //[TestMethod]
        private void TestPrintAndReload(params ITestCollection[] collections)
        {
            Csv csv = new Csv(testCsvPath);
            foreach (var col in collections)
            {
                foreach (var ele in col)
                    csv.SaveObject(ele, collectionId: col.ID);
            }
            csv.Save();
            Csv load = new Csv(testCsvPath);
            load.Load();

            List<string> errors = new List<string>();
            foreach (ITestCollection col in collections)
            {
                var length = col.Count;
                var loadedList = new List<object>();
                for (int i = 0; i < length; ++i)
                {
                    var originalObj = col[i];
                    var loadedObj = load.GetObject(col.Type, i, col.ID);
                    loadedList.Add(loadedObj);
                    if (!originalObj.Equals(loadedObj))
                        errors.Add($"Error deserializing element '{i}' with collection '{col.ID}'");
                }
            }

            Assert.IsTrue(errors.Count == 0, $"One or more reloads failed: {string.Join(",", errors)}");
        }

        [TestMethod]
        public void TestPrintSingleColumnCsv()
        {
            var test = new TestCollection<int>();
            for (int i = 0; i < 9; ++i)
            {
                test.Add(i);
            }
            TestPrintAndReload(test);
        }

        [TestMethod]
        public void TestPrintDoubleColumnCsv()
        {
            var t1 = new TestCollection<int>() { ID = "x" };
            var t2 = new TestCollection<int>() { ID = "Squared"};
            for (int i = 0; i < 9; ++i)
            {
                t1.Add(i);
                t2.Add(i * i);
            }
            TestPrintAndReload(t1, t2);
        }

        [TestMethod]
        public void TestPrintSimpleObjectCsv()
        {
            var csv = new Csv(testCsvPath);
            var originalList = new TestCollection<XYPoint>() { ID = "x" };
            for (int i = 0; i < 9; ++i)
            {
                var xyp = new XYPoint
                {
                    X = i,
                    Y = i * i
                };
                originalList.Add(xyp);
            }
            TestPrintAndReload(originalList);
        }

        [TestMethod]
        public void TestPrintClassCsv()
        {
            var test = new TestCollection<Alpha>();
            test.AddRange(alphas);
            TestPrintAndReload(test);
        }

        [TestMethod]
        public void TestPrintTwoCollectionsCsv()
        {
            var alphaTest = new TestCollection<Alpha>();
            alphaTest.AddRange(alphas);
            var bravoTest = new TestCollection<Bravo>() { ID = "BravoCollection" };
            bravoTest.AddRange(bravos);
            TestPrintAndReload(alphaTest, bravoTest);
        }

        [TestMethod]
        public void TestPrintPolyCsv()
        {
            Csv csv = new Csv(testCsvPath);
            var alphaTest = new TestCollection<Alpha>() { ID = "Alpha"};
            alphaTest.AddRange(alphas);
            alphaTest.AddRange(new[] 
            {
                new Alpha2(),
                new Alpha2() { Four = new Bravo() }
            });

            var bravoTest = new TestCollection<Bravo>() { ID = "BravoCollection" };
            bravoTest.AddRange(bravos);
            TestPrintAndReload(alphaTest, bravoTest);
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
