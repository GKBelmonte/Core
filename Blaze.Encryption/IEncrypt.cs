using Blaze.Encryption.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public interface IEncrypt
    {
        byte[] Encrypt(byte[] plain, byte[] key);
        byte[] Decrypt(byte[] cypher, byte[] key);
    }

    public interface IOperationEncrypt : IEncrypt
    {
        byte[] Encrypt(byte[] plain, byte[] key, Operation op);
        byte[] Decrypt(byte[] cypher, byte[] key, Operation op);
    }

    public interface ISeededEncrypt : IEncrypt
    {
        byte[] Encrypt(byte[] plain, IRng key);
        byte[] Decrypt(byte[] cypher, IRng key);

        string Encrypt(string plain, IRng key);
        string Decrypt(string cypher, IRng key);
    }

    public enum Operation
    {
        Add, Sub, Xor, Custom, ReverseCustom
    }
}
