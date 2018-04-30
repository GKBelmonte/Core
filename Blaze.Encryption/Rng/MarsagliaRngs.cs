using Blaze.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Rng
{
    /// <summary>
    /// Got them from Marsagliia Rngs http://www.cse.yorku.ca/~oz/marsaglia-rng.html
    /// A dozen different methods and explenations for my hearts content
    /// </summary>
    public abstract class MarsagliaRng : IRng
    {
        // stole this from https://referencesource.microsoft.com/#mscorlib/system/random.cs,92e3cf6e56571d5a,references, 
        // which they stole from Numerical Recipes in C (2nd Ed.)
        // so I guess its ok :D
        protected uint[] ArrayFromSeed(int seed)
        {
            const int MSEED = 161803398;
            int ii;
            uint mj, mk;
            //Initialize our Seed array.
            //This algorithm comes from Numerical Recipes in C (2nd Ed.)
            int subtraction = (seed == Int32.MinValue) ? Int32.MaxValue : Math.Abs(seed);
            uint[] seedArray = new uint[56];
            mj = (uint)(MSEED - subtraction);
            seedArray[55] = mj;
            mk = 1;
            for (int i = 1; i < 55; i++)
            {  //Apparently the range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                ii = (21 * i) % 55;
                seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += Int32.MaxValue;
                mj = seedArray[ii];
            }
            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    seedArray[i] -= seedArray[1 + (i + 30) % 55];
                    if (seedArray[i] < 0) seedArray[i] += Int32.MaxValue;
                }
            }
            return seedArray;
        }

        public abstract int Next();

        public int Next(int minValue, int maxValue)
        {
            int range = maxValue - minValue;
            int next = Next();
            return (next.UMod(range)) + minValue;
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public virtual void NextBytes(byte[] buffer)
        {
            //Cast to avoid sign extensions when shiftting
            uint next = (uint)Next();
            //we could do Next(0,256) and that would discard 3/4 bytes
            byte consumedCount = 0;
            for (int i = 0; i < buffer.Length; ++i)
            {
                byte b = (byte)(next & 0xFF);
                buffer[i] = b;

                //At every 4th byte consumed, get a new uint
                consumedCount++;
                if (consumedCount % 4 == 0)
                    next = (uint)Next();
                else
                    next = next >> 8;
            }
        }

        public double NextDouble()
        {
            return Next() * 2.328306e-10;
        }

        protected void InitalizeLaggedFibTable(uint[] ints)
        {
            for (int i = 0; i < 256; ++i)
                t[i] = (ints[Next(1, 55)] >> 3) ^ (ints[Next(1, 55)] << 5) ^ (ints[Next(1, 55)] >> 7);

        }

        protected void InitalizeSubWithBorrowTable(uint[] ints)
        {
            for (int i = 0; i < 256; ++i)
                tswb[i] = (ints[Next(1, 55)] >> 3) ^ (ints[Next(1, 55)] << 5) ^ (ints[Next(1, 55)] >> 7);
        }

        #region Marsaglia's stuff
        protected uint z = 362436069;
        protected uint znew()
        {
            return z = 36969 * (z & 65535) + (z >> 16);
        }
        protected uint w = 521288629;
        protected uint wnew()
        {
            return w = 18000 * (w & 65535) + (w >> 16);
        }

        /// <summary>
        /// The MWC generator concatenates two 16-bit multiply-
        /// with-carry generators, x(n)=36969x(n-1)+carry,
        /// y(n)=18000y(n-1)+carry mod 2^16, has period about
        /// 2^60 and seems to pass all tests of randomness.A
        /// favorite stand-alone generator---faster than KISS,
        /// which contains it.
        /// </summary>
        protected uint MWC()
        {
            return (znew() << 16) + wnew();
        }

        /// <summary>
        /// SHR3 is a 3-shift-register generator with period
        /// 2^32-1. It uses y(n)=y(n-1)(I+L^17)(I+R^13)(I+L^5),
        /// with the y's viewed as binary vectors, L the 32x32
        /// binary matrix that shifts a vector left 1, and R its
        /// transpose.SHR3 seems to pass all except those
        /// related to the binary rank test, since 32 successive
        /// values, as binary vectors, must be linearly
        /// independent, while 32 successive truly random 32-bit
        /// integers, viewed as binary vectors, will be linearly
        /// independent only about 29% of the time.
        /// </summary>
        protected uint SHR3()
        {
            jsr ^= (jsr << 17);
            jsr ^= (jsr >> 13);
            return jsr ^= (jsr << 5);
        }
        protected uint jsr = 123456789;

        /// <summary>
        /// CONG is a congruential generator with the widely used 69069
        /// multiplier: x(n)=69069x(n-1)+1234567. It has period
        /// 2^32. The leading half of its 32 bits seem to pass
        /// tests, but bits in the last half are too regular.
        /// </summary>
        protected uint CONG()
        {
            return jcong = 69069 * jcong + 1234567;
        }
        protected uint jcong = 380116160;

        /// <summary>
        /// The KISS generator, (Keep It Simple Stupid), is
        /// designed to combine the two multiply-with-carry
        /// generators in MWC with the 3-shift register SHR3 and
        /// the congruential generator CONG, using addition and
        /// exclusive-or.Period about 2^123.
        /// It is one of my favorite generators.
        /// </summary>
        protected uint KISS()
        {
            return MWC() ^ CONG() + SHR3();
        }

        /// <summary>
        /// FIB is the classical Fibonacci sequence
        /// x(n)=x(n-1)+x(n-2),but taken modulo 2^32.
        /// Its period is 3*2^31 if one of its two seeds is odd
        /// and not 1 mod 8. It has little worth as a RNG by
        /// itself, but provides a simple and fast component for
        /// use in combination generators.
        /// </summary>
        protected uint FIB()
        {
            b = a + b;
            return a = b - a;
        }
        protected uint a = 224466889;
        protected uint b = 7584631;

        /// <summary>
        /// LFIB4 is an extension of what I have previously
        /// defined as a lagged Fibonacci generator:
        /// x(n)=x(n-r) op x(n-s), with the x's in a finite
        /// set over which there is a binary operation op, such
        /// as +,- on integers mod 2^32, * on odd such integers,
        /// exclusive-or(xor) on binary vectors.Except for
        /// those using multiplication, lagged Fibonacci
        /// generators fail various tests of randomness, unless
        /// the lags are very long. (See SWB below).
        /// To see if more than two lags would serve to overcome
        /// the problems of 2-lag generators using +,- or xor, I
        /// have developed the 4-lag generator LFIB4 using
        /// addition: x(n)=x(n-256)+x(n-179)+x(n-119)+x(n-55)
        /// mod 2^32. Its period is 2^31* (2^256-1), about 2^287,
        /// and it seems to pass all tests---in particular,
        /// those of the kind for which 2-lag generators using
        /// +,-,xor seem to fail.For even more confidence in
        /// its suitability, LFIB4 can be combined with KISS,
        /// with a resulting period of about 2^410: just use
        /// (KISS+LFIB4) in any C expression.
        /// </summary>
        protected uint LFIB4()
        {
            c++;
            t[c] = t[c] 
                + t[(byte)(c + 58)] 
                + t[(byte)(c + 119)] 
                + t[(byte)(c + 178)];
            return t[c];
        }
        protected byte c = 0;
        protected uint[] t = new uint[256];

        /// <summary>
        /// SWB is a subtract-with-borrow generator that I
        /// developed to give a simple method for producing
        /// extremely long periods:
        /// x(n)=x(n-222)-x(n-237)- borrow mod 2^32.
        /// The 'borrow' is 0, or set to 1 if computing x(n-1)
        /// caused overflow in 32-bit integer arithmetic.This
        /// generator has a very long period, 2^7098(2^480-1),
        /// about 2^7578. It seems to pass all tests of
        /// randomness, except for the Birthday Spacings test,
        /// which it fails badly, as do all lagged Fibonacci
        /// generators using +,- or xor.I would suggest
        /// combining SWB with KISS, MWC, SHR3, or CONG.
        /// KISS+SWB has period >2^7700 and is highly
        /// recommended.
        /// Subtract-with-borrow has the same local behaviour
        /// as lagged Fibonacci using +,-,xor---the borrow
        /// merely provides a much longer period.
        /// SWB fails the birthday spacings test, as do all
        /// lagged Fibonacci and other generators that merely
        /// combine two previous values by means of =,- or xor.
        /// Those failures are for a particular case: m= 512
        /// birthdays in a year of n = 2 ^ 24 days.There are
        /// choices of m and n for which lags >1000 will also
        /// fail the test.A reasonable precaution is to always
        /// combine a 2-lag Fibonacci or SWB generator with
        /// another kind of generator, unless the generator uses
        /// *, for which a very satisfactory sequence of odd
        /// 32-bit integers results.
        /// </summary>
        protected uint SWB()
        {
            cswb++;
            bro = x < y ? 1u : 0u;
            tswb[c] = (x = tswb[(byte)(cswb + 34)]) - (y = tswb[(byte)(cswb + 19)] + bro);
            return tswb[c];
        }
        protected byte cswb = 0;
        protected uint bro = 0, x = 0, y = 0; //tee hee
        protected uint[] tswb = new uint[256];

        /* Any one of KISS, MWC, FIB, LFIB4, SWB, SHR3, or CONG
           can be used in an expression to provide a random 32-bit
           integer.

           The classical Fibonacci sequence mod 2^32 from FIB
           fails several tests. It is not suitable for use by
           itself, but is quite suitable for combining with
           other generators.

           The last half of the bits of CONG are too regular,
           and it fails tests for which those bits play a
           significant role. CONG+FIB will also have too much
           regularity in trailing bits, as each does. But keep
           in mind that it is a rare application for which
           the trailing bits play a significant role. CONG
           is one of the most widely used generators of the
           last 30 years, as it was the system generator for
           VAX and was incorporated in several popular
           software packages, all seemingly without complaint.

           Finally, because many simulations call for uniform
           random variables in 0<x<1 or -1<x<1, I use #define
           statements that permit inclusion of such variates
           directly in expressions: using UNI will provide a
           uniform random real (float) in (0,1), while VNI will
           provide one in (-1,1).

           All of these: MWC, SHR3, CONG, KISS, LFIB4, SWB, FIB
           UNI and VNI, permit direct insertion of the desired
           random quantity into an expression, avoiding the
           time and space costs of a function call. I call
           these in-line-define functions. To use them, static
           variables z,w,jsr,jcong,a and b should be assigned
           seed values other than their initial values. If
           LFIB4 or SWB are used, the static table t[256] must
           be initialized.

           A note on timing: It is difficult to provide exact
           time costs for inclusion of one of these in-line-
           define functions in an expression. Times may differ
           widely for different compilers, as the C operations
           may be deeply nested and tricky. I suggest these
           rough comparisons, based on averaging ten runs of a
           routine that is essentially a long loop:
           for(i=1;i<10000000;i++) L=KISS; then with KISS
           replaced with SHR3, CONG,... or KISS+SWB, etc. The
           times on my home PC, a Pentium 300MHz, in nanoseconds:
           FIB 49;LFIB4 77;SWB 80;CONG 80;SHR3 84;MWC 93;KISS 157;
           VNI 417;UNI 450;
        */
        #endregion Marsaglia's stuff
    }

    namespace Marsaglia
    {
        public class KissRng : MarsagliaRng
        {
            public KissRng() { }
            public KissRng(int seed)
            {
                var ints = ArrayFromSeed(seed);

                z = ints[1];
                w = ints[2];
                jsr = ints[3];
                jcong = ints[4];
            }
            public override int Next() { return (int)KISS(); }
        }

        public class MultiplyWithCarryRng : MarsagliaRng
        {
            public MultiplyWithCarryRng() { }
            public MultiplyWithCarryRng(int seed)
            {
                var ints = ArrayFromSeed(seed);

                z = ints[1];
                w = ints[2];
            }
            public override int Next() { return (int)MWC(); }
        }

        public class ShiftRegisterRng : MarsagliaRng
        {
            public ShiftRegisterRng() { }
            public ShiftRegisterRng(int seed)
            {
                var ints = ArrayFromSeed(seed);

                jsr = ints[1];
            }
            public override int Next() { return (int)SHR3(); }
        }

        public class CongruentialRng : MarsagliaRng
        {
            public CongruentialRng() { }
            public CongruentialRng(int seed)
            {
                var ints = ArrayFromSeed(seed);

                jcong = ints[1];
            }
            public override int Next() { return (int)CONG(); }
        }

        public class LaggedFibonacciRng : MarsagliaRng
        {
            public LaggedFibonacciRng() : this(0) { }
            public LaggedFibonacciRng(int seed)
            {
                uint[] ints = ArrayFromSeed(seed);

                InitalizeLaggedFibTable(ints);
            }
            public override int Next() { return (int)LFIB4(); }
        }

        public class SubstractWithBorrowRng : MarsagliaRng
        {
            public SubstractWithBorrowRng() : this(0) { }
            public SubstractWithBorrowRng(int seed)
            {
                var ints = ArrayFromSeed(seed);

                InitalizeSubWithBorrowTable(ints);
            }
            public override int Next() { return (int)SWB(); }
        }

        /// <summary>
        /// Mixed Shared State Random Marsaglia Rng
        /// </summary>
        public class MSSRMRng : MarsagliaRng
        {
            public MSSRMRng() : this(0) { }
            public MSSRMRng(int seed)
            {
                var ints = ArrayFromSeed(seed);

                z = ints[1];
                w = ints[2];
                jsr = ints[3];
                jcong = ints[4];

                InitalizeLaggedFibTable(ints);
                InitalizeSubWithBorrowTable(ints);
            }

            int switcher = 0;

            public override int Next()
            {
                switcher = (switcher+1) & 0b111;
                switch (switcher)
                {
                    case 0: return (int)KISS();
                    case 1: return (int)MWC();
                    case 2: return (int)SHR3();
                    case 3: return (int)SWB();
                    case 4: return (int)LFIB4();
                    case 5: return (int)CONG();
                    case 6: return (int)(KISS() ^ SHR3() + FIB());
                    case 7: return (int)(MWC() ^ SWB() + LFIB4());
                    default: throw new InvalidOperationException();
                }
            }
        }

    }
}
