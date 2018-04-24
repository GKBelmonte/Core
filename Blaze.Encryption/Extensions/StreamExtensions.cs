using Blaze.Cryptography.Rng;
using System;
using System.IO;
using System.Linq;


namespace Blaze.Cryptography.Extensions.Streams
{
    public static class StreamExtensions
    {
        public static void Encrypt(
            this ICypher self, 
            Stream plain, 
            byte[] key, 
            Func<int, int, int> op, 
            Stream outCypher)
        {
            if (!plain.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outCypher.CanWrite)
                throw new InvalidOperationException($"Output stream does not allow write");

            byte[] buff = new byte[1024];
            int read;
            while((read = plain.Read(buff, 0, 1024)) != 0)
            {
                byte[] bytesRead = buff.Take(read).ToArray();
                outCypher.Write(self.Encrypt(bytesRead, key, op), 0, read);
            }
        }

        public static void Decrypt(
            this ICypher self,
            Stream cypher,
            byte[] key,
            Func<int, int, int> reverseOp,
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
                outPlain.Write(self.Decrypt(bytesRead, key, reverseOp), 0, read);
            }
        }

        public static void Encrypt(
            this ICypher self,
            Stream plain,
            byte[] key,
            Stream outCypher)
        {
            Encrypt(self, 
                plain, 
                key, 
                Operations.OperationExtensions.GetOpFunc(Operation.Xor), 
                outCypher);
        }

        public static void Decrypt(
            this ICypher self,
            Stream cypher,
            byte[] key,
            Stream outPlain)
        {
            Decrypt(self,
                cypher,
                key,
                Operations.OperationExtensions.GetOpFunc(Operation.Xor),
                outPlain);
        }

        public static void Encrypt(
            this ICypher self,
            Stream plain,
            IRng key,
            Func<int, int, int> op,
            Stream outCypher)
        {
            if (!plain.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outCypher.CanWrite)
                throw new InvalidOperationException($"Input stream does not allow write");

            byte[] buff = new byte[1024];
            int read = plain.Read(buff, 0, 1024);
            while (read != 0)
                outCypher.Write(self.Encrypt(buff, key, op), 0, read);
        }

        public static void Decrypt(
            this ICypher self,
            Stream cypher,
            IRng key,
            Func<int, int, int> reverseOp,
            Stream outPlain)
        {
            if (!cypher.CanRead)
                throw new InvalidOperationException($"Input stream does not allow read");
            if (!outPlain.CanWrite)
                throw new InvalidOperationException($"Input stream does not allow write");

            byte[] buff = new byte[1024];
            int read = cypher.Read(buff, 0, 1024);
            while (read != 0)
                outPlain.Write(self.Decrypt(buff, key, reverseOp), 0, read);
        }

        public static void Encrypt(
            this ICypher self,
            Stream plain,
            IRng key,
            Stream outCypher)
        {
            Encrypt(self,
                plain,
                key,
                Operations.OperationExtensions.GetOpFunc(Operation.Xor),
                outCypher);
        }

        public static void Decrypt(
            this ICypher self,
            Stream cypher,
            IRng key,
            Stream outPlain)
        {
            Encrypt(self,
                cypher,
                key,
                Operations.OperationExtensions.GetOpFunc(Operation.Xor),
                outPlain);
        }
    }
}
