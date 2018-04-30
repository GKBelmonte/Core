using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Tests.Rng
{
    public class NullRng : MarsagliaRng
    {
        public NullRng(int s) { }
        public override int Next()
        {
            return 0;
        }
    }

    // For a cypher that uses a Rng to be good,
    // the same measures of confussion, diffusion and distribution are required by
    // the Rng itself
    //public class RngCypher : AlphabeticCypher
    //{
    //    protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
    //    {
    //        byte[] cypher = new byte[plain.Length];
    //    }
    //}
}
