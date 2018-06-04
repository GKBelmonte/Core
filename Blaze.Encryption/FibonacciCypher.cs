using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public class FibonacciCypher : AlphabeticCypher, ICypher
    {
        public FibonacciCypher() { }

        public FibonacciCypher(char[] alphabet)
        {
            Alphabet = alphabet;
        }

        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            byte[] keyHash = key.GetMD5Hash();
            int seed = keyHash.ToInt32();

            int[] p = BytesToIndices(plain);
            int[] c = new int[plain.Length];

            ForwardPass(seed, p, c);

            byte[] cypher = IndicesToBytes(c);
            return cypher;
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            byte[] keyHash = key.GetMD5Hash();
            int seed = keyHash.ToInt32();

            var c = BytesToIndices(cypher);
            var p = new int[cypher.Length];

            BackwardsPass(seed, c, p);

            byte[] plain = IndicesToBytes(p);
            return plain;
        }

        // Fibonacci cypher is such that  c[i+1] = p[i+1] + c[i]  |     c[i] = p[i] + c[i-1]
        //                            and c[0] = p[0] + key;
        // so p[i] = c[i] - c[i-1]
        // also means though that key = c[0] - p[0]
        protected virtual void ForwardPass(int seed, int[] p, int[] c)
        {
            c[0] = ForwardOp(p[0], seed);

            for (var ii = 1; ii < p.Length; ++ii)
            {
                int nextInx = ForwardOp(p[ii], c[ii - 1]);
                c[ii] = nextInx;
            }
        }

        // p[i] = c[i] - c[i-1]
        protected virtual void BackwardsPass(int seed, int[] c, int[] p)
        {
            p[0] = ReverseOp(c[0], seed);

            for (var ii = 1; ii < p.Length; ++ii)
            {
                int nextInx = ReverseOp(c[ii], c[ii - 1]);
                p[ii] = nextInx;
            }
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            //should not be needed
            throw new NotImplementedException();
        }
    }

    // weaker in confusion, but the cypher text depends solely on the plain,
    // so knowing the plain, requires to know the plain, so the key cannot be cracked trivially like v1
    public class FibonacciCypherV2 : FibonacciCypher
    {
        // Fibonacci cypher is such that  c[i+1] = p[i+1] + p[i]  |     c[i] = p[i] + p[i-1]
        //                            and c[0] = p[0] + key;
        // so p[i] = c[i] - p[i-1]
        protected override void ForwardPass(int seed, int[] p, int[] c)
        {
            c[0] = ForwardOp(p[0], seed);

            for (var ii = 1; ii < p.Length; ++ii)
            {
                int nextInx = ForwardOp(p[ii], p[ii - 1]);
                c[ii] = nextInx;
            }
        }

        // p[i] = c[i] - p[i-1]
        protected override void BackwardsPass(int seed, int[] c, int[] p)
        {
            p[0] = ReverseOp(c[0], seed);

            for (var ii = 1; ii < p.Length; ++ii)
            {
                int nextInx = ReverseOp(c[ii], p[ii - 1]);
                p[ii] = nextInx;
            }
        }
    }

    // v1 + v2
    public class FibonacciCypherV3 : FibonacciCypher
    {
        // Fibonacci cypher is such that  c[i+1] = p[i+1] + p[i] + c[i]  |     c[i] = p[i] + p[i-1] + c[i-1]
        //                            and c[0] = p[0] + key;
        // so p[i] = c[i] - c[i-1] - p[i-1]
        protected override void ForwardPass(int seed, int[] p, int[] c)
        {
            c[0] = ForwardOp(p[0], seed);

            for (var ii = 1; ii < p.Length; ++ii)
            {
                int nextInx = ForwardOp(ForwardOp(p[ii], p[ii - 1]), c[ii -1]);
                c[ii] = nextInx;
            }
        }

        // so p[i] = c[i] - c[i-1] - p[i-1]
        protected override void BackwardsPass(int seed, int[] c, int[] p)
        {
            p[0] = ReverseOp(c[0], seed);

            for (var ii = 1; ii < p.Length; ++ii)
            {
                int nextInx = ReverseOp(ReverseOp(c[ii], c[ii - 1]), p[ii-1]);
                p[ii] = nextInx;
            }
        }
    }
}
