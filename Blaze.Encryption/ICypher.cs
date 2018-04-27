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
        byte[] Encrypt(byte[] plain, byte[] key);

        /// <summary>
        /// Decrypts the bytes using another set of bytes as key.
        /// The operation to be used per byte of decryption, normally XOR since it is
        /// symmetric or SUB if ADD was used to encrpt.
        /// </summary>
        /// <returns>The decrypted bytes</returns>
        byte[] Decrypt(byte[] cypher, byte[] key);


        byte[] Encrypt(byte[] plain, IRng key);
        byte[] Decrypt(byte[] cypher, IRng key);

        /// <summary>
        /// The alphabet of allowed characters to encrypt and decrypt.
        /// By default, there's no restriction on what characters to use.
        /// </summary>
        IReadOnlyList<char> Alphabet { get; set; }

        /// <summary>
        /// Permits to override the default operation on blocks
        /// Should have a default such as XOR or ADD
        /// p == ReverseOp(k, ForwardOp(p)) == ForwardOp(k, ReverseOp(p))
        /// </summary>
        Op ForwardOp { get; set; }
        /// <summary>
        /// Permits to override the default reverse operation on blocks
        /// Should have a default such as XOR or SUB
        /// p == ReverseOp(k, ForwardOp(p)) == ForwardOp(k, ReverseOp(p))
        /// </summary>
        Op ReverseOp { get; set; }
    }

    public delegate int Op(int key, int inx);

    public enum BasicOperations
    {
        Add,
        Sub,
        Xor
    }
}
