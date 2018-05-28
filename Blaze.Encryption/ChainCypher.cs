using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public class ChainCypher : ICypher
    {
        private readonly IReadOnlyList<ICypher> _cyphers;

        public IReadOnlyList<char> _alphabet;
        public IReadOnlyList<char> Alphabet
        {
            get { return _alphabet; }
            set
            {
                _alphabet = value;
                foreach (ICypher c in _cyphers)
                    c.Alphabet = value;
            }
        }

        private Op _forwardOp;
        public Op ForwardOp
        {
            get { return _forwardOp; }
            set
            {
                foreach (ICypher c in _cyphers)
                    c.ForwardOp = value;
                _forwardOp = value;
            }
        }

        private Op _reverseOp;
        public Op ReverseOp
        {
            get { return _reverseOp; }
            set
            {
                foreach (ICypher c in _cyphers)
                    c.ReverseOp = value;
                _reverseOp = value;
            }
        }

        public ChainCypher(params Type[] types)
        {
            Type uninitializeableType = types.FirstOrDefault(t => t.GetConstructor(Type.EmptyTypes) == null);
            if (uninitializeableType != null)
                throw new ArgumentException($"Type {uninitializeableType.FullName} does not have a default constructor");

            _cyphers = types
                .Select(t => (ICypher)Activator.CreateInstance(t))
                .ToList();
        }

        public ChainCypher(params ICypher[] encrypts)
        {
            _cyphers = encrypts.ToList();
        }

        public byte[] Encrypt(byte[] plain, byte[] key)
        {
            List<byte[]> pepperedKeys = GetPepperedKeys(key);

            byte[] currentPass = new byte[plain.Length];
            plain.CopyTo(currentPass, 0);

            for (int i = 0; i < _cyphers.Count; ++i)
            {
                byte[] pepperedKey = pepperedKeys[i];
                currentPass = _cyphers[i].Encrypt(currentPass, pepperedKey);
            }

            return currentPass;
        }

        public byte[] Decrypt(byte[] cypher, byte[] key)
        {
            List<byte[]> pepperedKeys = GetPepperedKeys(key);

            byte[] currentPass = new byte[cypher.Length];
            cypher.CopyTo(currentPass, 0);

            for (int i = _cyphers.Count - 1; i >= 0; --i)
            {
                byte[] pepperedKey = pepperedKeys[i];
                currentPass = _cyphers[i].Decrypt(currentPass, pepperedKey);
            }

            return currentPass;
        }

        public byte[] Encrypt(byte[] plain, IRng key)
        {
            throw new NotSupportedException();
            //byte[] currentPass = new byte[plain.Length];
            //plain.CopyTo(currentPass, 0);
            //for (int i = 0; i < _cyphers.Count; ++i)
            //{
            //    currentPass = _cyphers[i].Encrypt(currentPass, key);
            //}

            //return currentPass;
        }

        public byte[] Decrypt(byte[] cypher, IRng key)
        {
            throw new NotSupportedException();
            //This won't work S:(

            //byte[] currentPass = new byte[cypher.Length];
            //cypher.CopyTo(currentPass, 0);

            //for (int i = _cyphers.Count - 1; i >= 0; --i)
            //{
            //    currentPass = _cyphers[i].Decrypt(currentPass, key, op);
            //}

            //return currentPass;
        }

        /// <summary>
        /// For each _cypher generate a peppered key from the original key
        /// Re-using the key could leak statistical information if the key
        /// is used for a Rng based cypher where the rng values would be
        /// generated multiple times
        /// </summary>
        private List<byte[]> GetPepperedKeys(byte[] key)
        {
            byte[] hashedKey = key.GetMD5Hash();
            IRng random = hashedKey.KeyToRand();
            byte[] discard = new byte[random.Next(100, 111)];
            random.NextBytes(discard);

            List<byte[]> pepperedKeys = _cyphers
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
