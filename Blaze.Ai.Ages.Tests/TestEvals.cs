using Blaze.Ai.Ages;
using Blaze.Core.Extensions;
using Blaze.Core.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Ai.Ages.Tests
{
    [TestClass]
    public class TestEvals
    {
        ILogger Log = new ConsoleLogger();

        public class DInd : IIndividual
        {
            public double Val { get; }
            public DInd(double d)
            {
                Val = d;
            }



            public string Name => "";

            public IIndividual Mutate(float probability, float sigma)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void TestQuickTournament()
        {
            var inds = Enumerable
                .Range(0, 10)
                .Select(i => new DInd(i))
                .ToList();

            inds.Shuffle(new Random(0));

            List<DInd> sortedInds = null;

            QuickTournament q = new QuickTournament(
                inds,
                (l, r) => (float)(((DInd)l).Val - ((DInd)r).Val),
                (sinds) => sortedInds = sinds.Cast<DInd>().ToList() );

            q.Start();

            Assert.IsTrue(q.DrawCount == 0);

            for (int i = 0; i < sortedInds.Count - 1; ++i)
            {
                Assert.IsTrue(sortedInds[i].Val < sortedInds[i+1].Val);
                Assert.IsTrue(sortedInds[i].Val + 1 == sortedInds[i + 1].Val);
            }

        }
    }
}
