﻿using Blaze.Encryption.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Encryption
{
    public class ChainCypher : IEncrypt
    {
        private readonly IReadOnlyList<IEncrypt> _encrypts;
        public ChainCypher(params Type[] types)
        {
            Type uninitializeableType = types.FirstOrDefault(t => t.GetConstructor(Type.EmptyTypes) == null);
            if (uninitializeableType != null)
                throw new ArgumentException($"Type {uninitializeableType.FullName} does not have a default constructor");

            _encrypts = types
                .Select(t => (IEncrypt)Activator.CreateInstance(t))
                .ToList();
        }

        public ChainCypher(params IEncrypt[] encrypts)
        {
            _encrypts = encrypts.ToList();
        }

        public virtual byte[] Decrypt(byte[] cypher, byte[] key)
        {
            List<byte[]> pepperedKeys = GetPepperedKeys(key);

            byte[] currentPass = new byte[cypher.Length];
            cypher.CopyTo(currentPass, 0);

            for (int i = _encrypts.Count -1; i >= 0; --i)
            {
                byte[] pepperedKey = pepperedKeys[i];
                currentPass = _encrypts[i].Decrypt(currentPass, pepperedKey);
            }

            return currentPass;
        }

        public virtual byte[] Encrypt(byte[] plain, byte[] key)
        {
            List<byte[]> pepperedKeys = GetPepperedKeys(key);

            byte[] currentPass = new byte[plain.Length];
            plain.CopyTo(currentPass, 0);

            for (int i = 0; i < _encrypts.Count; ++i)
            {
                byte[] pepperedKey = pepperedKeys[i];
                currentPass = _encrypts[i].Encrypt(currentPass, pepperedKey);
            }

            return currentPass;
        }

        private List<byte[]> GetPepperedKeys(byte[] key)
        {
            byte[] hashedKey = key.GetMD5Hash();
            IRng random = hashedKey.KeyToRand();
            byte[] discard = new byte[random.Next(100, 111)];
            random.NextBytes(discard);

            List<byte[]> pepperedKeys = _encrypts
                .Select(e =>
                {
                    byte[] pepper = new byte[hashedKey.Length];
                    random.NextBytes(pepper);
                    return hashedKey.Pepper(pepper);
                })
                .ToList();
            return pepperedKeys;
        }
    }
}
