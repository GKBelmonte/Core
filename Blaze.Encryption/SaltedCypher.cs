using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography
{
    public class SaltedCypher<TCypher> : AlphabeticCypher where TCypher : ICypher
    {
        ICypher _cypher;
        public SaltedCypher(ICypher inner)
        {
            _cypher = inner;
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            throw new NotImplementedException();
        }

    }
}
