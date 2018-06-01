using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blaze.Cryptography.Tests.Rng
{
    [TestClass]
    public class GenerationTests
    {
        [TestMethod]
        public void RC4Generation()
        {
            //Test specified generation
            IRng rng = new RC4Rng("Key".ToByteArray());
            byte[] nexts = new byte[10];
            rng.NextBytes(nexts);
            byte[] expectedNexts = { 0xEB, 0x9F, 0x77, 0x81, 0xB7, 0x34, 0xCA, 0x72, 0xA7, 0x19 };

            for (int i = 0; i < nexts.Length; ++i)
                Assert.AreEqual(expectedNexts[i], nexts[i]);
        }
    }
}
