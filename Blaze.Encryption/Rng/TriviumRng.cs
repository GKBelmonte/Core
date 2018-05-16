using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Rng
{
    //as per https://www.springer.com/cda/content/document/cda_downloaddocument/9783642041006-c1.pdf
    public class TriviumRng : ByteRng
    {
        //Specification of Trivium
        //  register    length  feedback bit    feedforward bit     AND inputs
        //  A           93      69              66                  91, 92
        //  B           84      78              69                  82, 83
        //  C           111     87              66                  109, 110

        private ulong _lA;
        private ulong _hA;

        private ulong _lB;
        private ulong _hB;

        private ulong _lC;
        private ulong _hC;

        public TriviumRng(int seed)
        {
            Init(seed);
        }

        public TriviumRng(byte[] seeds)
        {
            if (seeds.Length <= 4)
                Init(seeds.ToSeed());

            int halfLength = seeds.Length / 2;

            Init(seeds.Take(halfLength).ToArray(), 
                seeds.Skip(halfLength).ToArray());
        }

        public TriviumRng(byte[] key, byte[] iv)
        {
            Init(key, iv);
        }

        private void Init(int seed)
        {
            uint[] arr = RngHelpers.ArrayFromSeed(seed);
            Init(arr.Skip(1)
                    .Take(10)
                    .Select(i => (byte) (i ^ (i >> 8 )^ (i >> 16) ^ (i >> 24)))
                    .ToArray(),
                arr.Skip(11)
                    .Take(10)
                    .Select(i => (byte)(i ^ (i >> 8) ^ (i >> 16) ^ (i >> 24)))
                    .ToArray());
        }

        private void Init(byte[] k, byte[] iv)
        {
            _lA = 0;
            _hA = 0;
            _lB = 0;
            _hB = 0;
            _lC = 0;
            _hC = 0; 
            for (int i = 0; i < iv.Length && i < 8; ++i)
                _lA |= ((ulong)iv[i]) << (i*8);

            for (int i = 8; i < iv.Length && i < 10; ++i)
                _hA |= (ulong)iv[i] << ((i-8) * 8);

            for (int i = 0; i < k.Length && i < 8; ++i)
                _lB |= (ulong)k[i] << (i * 8);

            for (int i = 8; i < k.Length && i < 10; ++i)
                _hB |= (ulong)k[i] << ((i - 8) * 8);

            _lC = 0;
            //C's 3 msb are set
            _hC = 3 << (109 - 64);

            byte warmup;
            for (int i = 0; i < 4 * 288; ++i)
                warmup = NextByte();
        }

        public override byte NextByte()
        {
            byte next = 0;

            for (byte bitNum = 0; bitNum < 8; ++bitNum)
            {
                //3 most significant bits
                ulong abits = _hA >> (91 - 64);
                ulong bbits = _hB >> (82 - 64);
                ulong cbits = _hC >> (109 - 64);

                //Get register next
                byte a = FeedForward(abits, _hA >> (66 - 64));
                byte b = FeedForward(bbits, _hB >> (69 - 64));
                byte c = FeedForward(cbits, _hC >> (66 - 64));

                //new bit
                byte n = (byte) (((a ^ b ^ c) & 1) << bitNum);

                //Shift new low-sig-bit
                ulong lsba = ((_hA >> (69 - 64)) ^ c) & 1;
                ulong lsbb = ((_hA >> (78 - 64)) ^ a) & 1;
                ulong lsbc = ((_hA >> (87 - 64)) ^ b) & 1;

                //Shift up 1
                _hA = _hA << 1;
                _hB = _hB << 1;
                _hC = _hC << 1;

                //lsb hX should be zero
                _hA |= _lA >> 63;
                _hB |= _lB >> 63;
                _hC |= _lC >> 63;

                _lA = _lA << 1 | lsba;
                _lB = _lB << 1 | lsbb;
                _lC = _lC << 1 | lsbc;

                next |= n;
            }

            return next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte FeedForward(ulong r, ulong ffbit)
        {
            //cpu beats memory
            return (byte) ((
                (r >> 2)            //bit 2 ^
                ^ ((r >> 1) & r))   //bit 1 * bit 0
                ^ ffbit );          // ^ feedforward
        }

    }
}
