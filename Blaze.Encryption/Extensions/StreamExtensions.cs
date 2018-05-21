using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Blaze.Cryptography.Extensions.Streams
{
    /// <summary>
    /// These extensions will fail for cyphers with feedback loops with more bytes than
    /// the number of characters
    /// </summary>
    public static class StreamExtensions
    {
        public static void Encrypt(
            this ICypher self,
            Stream plain,
            byte[] key,
            Stream outCypher)
        {
            if (!plain.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outCypher.CanWrite)
                throw new InvalidOperationException($"Output stream does not allow write");

            byte[] buff = new byte[1024];
            int read;
            while ((read = plain.Read(buff, 0, 1024)) != 0)
            {
                byte[] bytesRead = buff.Take(read).ToArray();
                byte[] bytesWrite = self.Encrypt(bytesRead, key);
                outCypher.Write(bytesWrite, 0, bytesWrite.Length);
            }
        }

        public static void Decrypt(
            this ICypher self,
            Stream cypher,
            byte[] key,
            Stream outPlain)
        {
            if (!cypher.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outPlain.CanWrite)
                throw new InvalidOperationException($"Output stream does not allow write");

            byte[] buff = new byte[1024];
            int read;
            while ((read = cypher.Read(buff, 0, 1024)) != 0)
            {
                byte[] bytesRead = buff.Take(read).ToArray();
                byte[] bytesWrite = self.Decrypt(bytesRead, key);
                outPlain.Write(bytesWrite, 0, bytesWrite.Length);
            }
        }



        public static void Encrypt(
            this ICypher self,
            Stream plain,
            IRng key,
            Stream outCypher)
        {
            if (!plain.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outCypher.CanWrite)
                throw new InvalidOperationException($"Output stream does not allow write");

            byte[] buff = new byte[1024];
            int read;
            while ((read = plain.Read(buff, 0, 1024)) != 0)
            {
                byte[] bytesRead = buff.Take(read).ToArray();
                byte[] bytesWrite = self.Encrypt(bytesRead, key);
                outCypher.Write(bytesWrite, 0, bytesWrite.Length);
            }
        }

        public static void Decrypt(
            this ICypher self,
            Stream cypher,
            IRng key,
            Stream outPlain)
        {
            if (!cypher.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outPlain.CanWrite)
                throw new InvalidOperationException($"Output stream does not allow write");

            byte[] buff = new byte[1024];
            int read;
            while ((read = cypher.Read(buff, 0, 1024)) != 0)
            {
                byte[] bytesRead = buff.Take(read).ToArray();
                byte[] bytesWrite = self.Decrypt(bytesRead, key);
                outPlain.Write(bytesWrite, 0, bytesWrite.Length);
            }
        }

    }
}
