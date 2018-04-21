using Blaze.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    /// <summary>
    /// Implements trivial string overloads of IEncrypt
    /// </summary>
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

        /// <summary>
        /// Optionally do stuff to the key before using it.
        /// Right now, it hashes it
        /// </summary>
        protected virtual byte[] ProcessKeyInternal(byte[] key)
        {
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
