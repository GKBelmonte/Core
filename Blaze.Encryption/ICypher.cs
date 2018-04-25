using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;

namespace Blaze.Cryptography
{
    public interface ICypher
    {
        /// <summary>
        /// Encrypts the bytes using another set of bytes as key.
        /// The operation to be used per byte of encryption, normally XOR
        /// </summary>
        /// <returns>The encrypted bytes</returns>
        byte[] Encrypt(byte[] plain, byte[] key, Func<int,int,int> op);
        
        /// <summary>
        /// Decrypts the bytes using another set of bytes as key.
        /// The operation to be used per byte of decryption, normally XOR since it is
        /// symmetric or SUB if ADD was used to encrpt.
        /// </summary>
        /// <returns>The decrypted bytes</returns>
        byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> reverseOp);


        byte[] Encrypt(byte[] plain, IRng key, Func<int, int, int> op);
        byte[] Decrypt(byte[] cypher, IRng key, Func<int, int, int> reverseOp);

        /// <summary>
        /// The alphabet of allowed characters to encrypt and decrypt.
        /// By default, there's no restriction on what characters to use.
        /// </summary>
        IReadOnlyList<char> Alphabet { get; set; }
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
