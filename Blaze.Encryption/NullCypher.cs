using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    //Does the operation on the plain, ignoring the key (treating as 0)
    public class NullCypher : AlphabeticCypher, ICypher
    {
        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            var pIx = ByteToIndices(plain);
            var cx = pIx
                .Select(px => op(px, 0))
                .ToArray();
            return IndicesToBytes(cx);
        }
    }
}
