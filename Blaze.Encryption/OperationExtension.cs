using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public static class OperationExtension
    {
        public static byte[] Encrypt(this IEncrypt self, byte[] plain, byte[] key)
        {
            var f = GetOpFunc(Operation.Add);
            return self.Encrypt(plain, key, f);
        }

        public static byte[] Decrypt(this IEncrypt self, byte[] cypher, byte[] key)
        {
            var refF = GetOpFunc(Operation.Sub);
            return self.Decrypt(cypher, key, refF);
        }

        public static byte[] Encrypt(this IEncrypt self, byte[] plain, byte[] key, Operation op)
        {
            return self.Encrypt(plain, key, GetOpFunc(op));
        }

        public static byte[] Decrypt(this IEncrypt self, byte[] cypher, byte[] key, Operation op)
        {
            return self.Decrypt(cypher, key, GetOpFunc(op));
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
    }
}
