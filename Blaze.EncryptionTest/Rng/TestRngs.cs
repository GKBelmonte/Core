using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Tests.Rng
{
    public class NullRng : Int32Rng
    {
        public NullRng(int s) { }
        public override int Next()
        {
            return 0;
        }
    }

    public class FlushRng : Int32Rng
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
}
