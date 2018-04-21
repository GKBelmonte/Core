using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    //Does the operation on the plain, ignoring the key (treating as 0)
    public class NullCypher : AlphabeticEncrypt, IOperationEncrypt
    {
        public override byte[] Encrypt(byte[] plain, byte[] key, Operation op)
        {
            var f = GetOpFunc(op);
            var pIx = ByteToIndices(plain);
            var cx = pIx.Select(px => f(0, px)).ToArray();
            return IndicesToBytes(pIx);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Operation op)
        {
            return Encrypt(cypher, key, op.GetReverse());
        }
    }
}
