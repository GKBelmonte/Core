using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Tests
{
    public class TestCase
    {
        public TestCase(ICypher b, string a, float? c)
        {
            Name = a;
            Cypher = b;
            ExpectedVal = c;
        }
        public string Name { get; set; }
        public ICypher Cypher { get; set; }
        public float? ExpectedVal { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
