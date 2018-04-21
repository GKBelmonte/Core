using Blaze.Core.Extensions;
using Blaze.Encryption.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public abstract class SeededEncryptBase : AlphabeticEncrypt, ISeededEncrypt
    {
        public abstract byte[] Decrypt(byte[] cypher, IRng key);

        public abstract byte[] Encrypt(byte[] plain, IRng key);

        public string Decrypt(string cypher, IRng key)
        {
            byte[] cypherByte = cypher.ToByteArray();
            return Decrypt(cypherByte, key).ToTextString();
        }

        public string Encrypt(string plain, IRng key)
        {
            byte[] plainByte = plain.ToByteArray();
            return Encrypt(plainByte, key).ToTextString();
        }
    }
}
