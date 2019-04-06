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

        [TestMethod]
        public void TestCsv()
        {
            Csv csv = new Csv(@"E:\test\t.csv");
            csv.SaveObject(alphas[0], 0);
            csv.SaveObject(alphas[1], 1);
            csv.Save();
        }

        [TestMethod]
        public void TestTwoCollectionsCsv()
        {
            Csv csv = new Csv(@"E:\test\t.csv");
            csv.SaveObject(alphas[0], 0);
            csv.SaveObject(alphas[1], 1);
            foreach (var b in bravos)
                csv.SaveObject(b, collectionId: "BravoCollection");
            csv.Save();
        }

        [TestMethod]
        public void TestPoly()
        {
            Csv csv = new Csv(@"E:\test\t.csv");
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
    }
}
