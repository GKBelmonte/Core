using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Extensions.Operations
{
    public static class OperationExtensions
    {
        public static byte[] Encrypt(this ICypher self, byte[] plain, byte[] key, Operation op)
        {
            var fop = GetOpFunc(op);
            return self.Encrypt(plain, key, fop);
        }

        public static byte[] Decrypt(this ICypher self, byte[] cypher, byte[] key, Operation op)
        {
            var fop = GetOpFunc(op);
            return self.Decrypt(cypher, key, fop);
        }

        public static byte[] Encrypt(this ICypher self, byte[] plain, IRng key, Operation op)
        {
            var f = GetOpFunc(Operation.Xor);
            return self.Encrypt(plain, key, f);
        }

        public static byte[] Decrypt(this ICypher self, byte[] cypher, IRng key, Operation op)
        {
            var f = GetOpFunc(Operation.Xor);
            return self.Encrypt(cypher, key, f);
        }

        public static Func<int, int, int> GetOpFunc(this Operation op)
        {
            Func<int, int, int> d = null;
            switch (op)
            {
                case Operation.Add:
                    d = (a, b) => a + b;
                    break;
                case Operation.Sub:
                    d = (a, b) => a - b;
                    break;
                case Operation.Xor:
                    d = (a, b) => a ^ b;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }
            return d;
        }

        public static Operation GetReverse(this Operation op)
        {
            switch (op)
            {
                case Operation.Add:
                    return Operation.Sub;
                case Operation.Sub:
                    return Operation.Add;
                case Operation.Xor:
                    return Operation.Xor;
                case Operation.Custom:
                    return Operation.ReverseCustom;
                case Operation.ReverseCustom:
                    return Operation.Custom;
                default:
                    throw new InvalidOperationException(string.Format("Cannot reverse unknown op '{0}'", op));
            }
        }

    }
}
