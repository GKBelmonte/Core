using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    //Does the operation on the plain, ignoring the key (treating as 0)
    public class NullCypher : AlphabeticEncrypt, IEncrypt
    {
        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int,int,int> f)
        {
            var pIx = ByteToIndices(plain);
            var cx = pIx
                .Select(px => f(px, 0))
                .ToArray();
            return IndicesToBytes(cx);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> f)
        {
            return Encrypt(cypher, key, f);
        }
    }
}
