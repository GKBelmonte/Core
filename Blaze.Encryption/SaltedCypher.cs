using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography
{
    /// <summary>
    /// I will call all nonce IVs salt, because the concept is almost the
    /// same and I enjoy the cooking analogy
    /// </summary>
    public class SaltedCypher : AlphabeticCypher
    {
        int _saltLength;
        ICypher _cypher;

        public Shaker Shaker { get; set; }

        public SaltedCypher(ICypher inner, int saltLength = 20)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            _cypher = inner;
            _saltLength = saltLength;
            Shaker = new Shaker();
        }

        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            byte[] salt, newKey;
            salt = Shaker.PinchOfSalt(_saltLength);
            Shaker.SaltKey(key, salt, out newKey);

            byte[] encodedSalt = IndicesToBytes(CryptoUtils.EncodeBytes(salt, Alphabet.Count).ToList());

            byte[] res = _cypher.Encrypt(plain, newKey);
            
            return encodedSalt.Concat(res).ToArray();
        }

        public override byte[] Decrypt(byte[] plain, byte[] key)
        {
            byte[] salt, newKey;
            int skipCount;
            salt = CryptoUtils.DecodeBytes(BytesToIndices(plain), Alphabet.Count, out skipCount);

            Shaker.SaltKey(key, salt, out newKey);

            byte[] res = _cypher.Decrypt(plain.Skip(skipCount).ToArray(), newKey);
            return res;
        }

        public override IReadOnlyList<char> Alphabet
        {
            get { return _cypher.Alphabet; }
            set
            {
                base.Alphabet = value;
                if(_cypher != null)
                    _cypher.Alphabet = value;
            }
        }

        public override Op ForwardOp
        {
            get { return _cypher.ForwardOp; }
            set { if (_cypher != null) _cypher.ForwardOp = value; }
        }

        public override Op ReverseOp
        {
            get => _cypher.ReverseOp;
            set { if (_cypher != null) _cypher.ReverseOp = value; }
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            throw new NotImplementedException();
        }
    }
}
