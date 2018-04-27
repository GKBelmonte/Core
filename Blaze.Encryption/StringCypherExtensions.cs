using Blaze.Core.Extensions;
using Blaze.Cryptography.Extensions.Operations;
using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    /// <summary>
    /// Implements trivial string overloads of IEncrypt
    /// </summary>
    public static class StringCypherExtensions
    {
        public static string Encrypt(this ICypher self, string plain, string key)
        {
            byte[] plainByte = plain.ToByteArray();
            byte[] keyByte = key.ToByteArray();
            return self.Encrypt(plainByte, keyByte).ToTextString();
        }

        public static string Decrypt(this ICypher self, string cypher, string key)
        {
            byte[] cypherByte = cypher.ToByteArray();
            byte[] keyByte = key.ToByteArray();
            return self.Decrypt(cypherByte, keyByte).ToTextString();
        }

        //Encrypt XOR only works perfectly if the alphabet size is a power of 2
        // a ^ k will never be bigger than the largest number with as many bits.
        // if a ^ k > |Alpha| then a ^ k % |Alpha|  =/= a ^ k and so we break bijection and we can't get 'a' back.
        // for example suppose Alpha = {00,01,10} hence |Alpha| = 3, 
        // If we observe the table given by (a ^ k) % |A| we notice there are duplicates, so bijection does not hold
        // for example 01 ^ 01 = 00 and 01 ^ 10 = 11 == 00 MOD 11
        // so (1 ^ 1) % 3 == (1 ^ 2) % 5 and there's no way to destinguish which plain text is correct

        //public static string Encrypt(this ICypher self, string plain, string key, BasicOperations op)
        //{
        //    byte[] plainByte = plain.ToByteArray();
        //    byte[] keyByte = key.ToByteArray();
        //    return self.Encrypt(plainByte, keyByte, op)
        //        .ToTextString();
        //}

        //public static string Decrypt(this ICypher self, string cypher, string key, BasicOperations op)
        //{
        //    byte[] cypherByte = cypher.ToByteArray();
        //    byte[] keyByte = key.ToByteArray();
        //    return self.Decrypt(cypherByte, keyByte, op)
        //        .ToTextString();
        //}

        //public static string Encrypt(this ICypher self, string plain, IRng key, BasicOperations op)
        //{
        //    byte[] plainByte = plain.ToByteArray();
        //    return self.Encrypt(plainByte, key, op).ToTextString();
        //}

        //public static string Decrypt(this ICypher self, string cypher, IRng key, BasicOperations op)
        //{
        //    byte[] cypherByte = cypher.ToByteArray();
        //    return self.Encrypt(cypherByte, key, op).ToTextString();
        //}
    }
}
