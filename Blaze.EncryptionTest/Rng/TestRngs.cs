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

    public class FlushRng : MarsagliaRng
    {
        int _state = 0;
        public FlushRng(int s) { }
        public override int Next() { return _state++; }

        public override void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; ++i)
                buffer[i] = (byte)i;
        }
    }

    // For a cypher that uses a Rng to be good,
    // the same measures of confussion and distribution are required by
    // the Rng itself
    public class RngCypher<T> : AlphabeticCypher where T : IRng
    {
        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            int seed = key.ToSeed();
            IRng rng = (IRng)Activator.CreateInstance(typeof(T), seed);
            byte[] cypher = new byte[plain.Length];
            rng.NextBytes(cypher);
            return cypher;
        }
    }
}
