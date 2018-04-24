using Blaze.Cryptography.Rng;
using System;

namespace Blaze.Cryptography
{
    public interface ICypher
    {
        byte[] Encrypt(byte[] plain, byte[] key, Func<int,int,int> op);
        byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> reverseOp);
        byte[] Encrypt(byte[] plain, IRng key, Func<int, int, int> op);
        byte[] Decrypt(byte[] cypher, IRng key, Func<int, int, int> reverseOp);
    }

    public enum Operation
    {
        Add,
        Sub,
        Xor,
        Custom,
        ReverseCustom
    }
}
