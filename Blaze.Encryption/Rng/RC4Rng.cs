using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Rng
{
    public class RC4Rng : ByteRng
    {
        private byte[] _State;
        private byte indexI;
        private byte indexJ;

        public RC4Rng(byte[] seed)
        {
            KeySchedulingAlgorithm(seed);
        }

        private void KeySchedulingAlgorithm(byte[] seed)
        {
            //Make a permutation from the seed/key
            _State = Enumerable
                    .Range(0, 256)
                    .Select(i => (byte)i)
                    .ToArray();
            int j = 0;
            for (int i = 0; i < 256; ++i)
            {
                j = (j + _State[i] + seed[i % seed.Length]) & 0xFF;
                Swap(j, i);
            }
        }

        public RC4Rng(int seed)
        {
            KeySchedulingAlgorithm(BitConverter.GetBytes(seed));
        }

        private void Swap(int j, int i)
        {
            byte si = _State[i];
            _State[i] = _State[j];
            _State[j] = si;
        }

        public override byte NextByte()
        {
            indexI++;
            indexJ += _State[indexI];
            Swap(indexI, indexJ);
            return _State[(byte)(_State[indexI] + _State[indexJ])];
        }
    }
}
