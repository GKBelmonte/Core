using Blaze.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public abstract class BaseCypher : IEncrypt
    {
        public virtual string Encrypt(string plain, string key)
        {
            byte[] plainByte = plain.ToByteArray();
            byte[] keyByte = key.ToByteArray();
            return Encrypt(plainByte, keyByte).ToTextString();
        }

        public virtual string Decrypt(string cypher, string key)
        {
            byte[] cypherByte = cypher.ToByteArray();
            byte[] keyByte = key.ToByteArray();
            return Decrypt(cypherByte, keyByte).ToTextString();
        }

        public abstract byte[] Encrypt(byte[] plain, byte[] key);
        public abstract byte[] Decrypt(byte[] cypher, byte[] key);


        public Func<byte[], byte[]> ProcessKey { get; set; }

        protected byte[] ProcessKeyInternal(byte[] key)
        {
            if (ProcessKey != null)
                return ProcessKey(key);

            byte[] keyHash;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                //seems pointless to salt key in this contetx
                //byte[] salt = key.ToTextString().GetHashCode().ToString().ToByteArray();
                var saltedKey = new List<byte>(key);
                //saltedKey.AddRange(salt);
                keyHash = md5.ComputeHash(saltedKey.ToArray());
            }
            return keyHash;
        }
    }
}
