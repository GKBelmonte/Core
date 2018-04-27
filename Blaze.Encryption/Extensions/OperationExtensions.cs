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
        public static Op GetOpFunc(this BasicOperations op)
        {
            Op d = null;
            switch (op)
            {
                case BasicOperations.Add:
                    d = (a, b) => a + b;
                    break;
                case BasicOperations.Sub:
                    d = (a, b) => a - b;
                    break;
                case BasicOperations.Xor:
                    d = (a, b) => a ^ b;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }
            return d;
        }

        public static BasicOperations GetReverse(this BasicOperations op)
        {
            switch (op)
            {
                case BasicOperations.Add:
                    return BasicOperations.Sub;
                case BasicOperations.Sub:
                    return BasicOperations.Add;
                case BasicOperations.Xor:
                    return BasicOperations.Xor;
                default:
                    throw new InvalidOperationException(string.Format("Cannot reverse unknown op '{0}'", op));
            }
        }

    }
}
