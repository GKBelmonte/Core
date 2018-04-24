using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;

namespace Blaze.Cryptography
{
    public class RandomBijection : AlphabeticCypher
    {
        private AlphabeticCypher _Encrypt;
        /*
        The bijections are normally of form p + k = c or p ^ k = c or p - k = c
         which are reversible by -,^ and + respectively.This causes weaknesses in cyphers like
         the Stream cypher where if we have to plain texts a and b, and both are encoded with the same
         key, we can have a ^ k and b ^ k which can be combined to give a ^ b which is a simpler
         running key cypher.
         However, if the attacker does not know the bijective function, this form of attack becomes much
         harder even if the key is re-used.
         The function used to treat the plaint text to get the cypher text is often a bijection(hence reversible)
         I assume that not knowing the function used to encrypt will make it signficiantly harder to crack,
         specially if the function is derived from the key.
         (note a bijection is function that maps one element to another, and hence is necessarily revertable
          like a map.
          In this case, this should be a multi-variable bijection
          so that if f(a, b) is known and either a or b is known, we can know b or a)
          Note: After further thought I realize this:
          This is still vulnerable to chosen plain text attack, since
          that may reveal the bijection if they key is re-used, which
          might in turn be used to derive the seed.
          (if the system allows encrypting arbitrary plaintext without requesting a key and reuses it internally)
        
          bijection is:
            f(p, k) = c OR _Bijection[k].Forward[p] = c
            f(c, k) = p OR _Bijection[k].Reverse[c] = p
         Because the bijection is generated, it most likely won't be commutative
         That is: f(p, k) != f(k, p), unlike f = p + k and f = p ^ k

         */
        private List<Map<int, int>> _Bijection;
        /// <summary>
        /// Use this constructor as an Decorator to the encryption that allows a custom operation.
        /// This will generate a random bijection so the attacker wont know whether xor or + is being used.
        /// </summary>
        /// <param name="enc"></param>
        public RandomBijection(AlphabeticCypher enc)
        {
            _Encrypt = enc;
            Alphabet = enc.Alphabet;
            ModulateKey = true;
        }

        /// <summary>
        /// Use this constructor to manually set the functions on the encryption
        /// enc.CustomOp = rbjd.Forward;
        /// enc.ReverseOp = rbjd.Reverse;
        /// </summary>
        /// <example>
        /// var rbjd = new RandomBijectionDecorator(enc.Alphabet)
        /// enc.InitializeBijection(rng);
        /// enc.CustomOp = rbjd.Forward;
        /// enc.ReverseOp = rbjd.Reverse;
        /// </example>
        /// <param name="alph"></param>
        public RandomBijection(char[] alph)
        {
            _Encrypt = new NullCypher();
            _Encrypt.Alphabet = alph;
            Alphabet = alph;
            ModulateKey = true;
        }

        public override char[] Alphabet
        {
            get
            {
                return base.Alphabet;
            }
            set
            {
                base.Alphabet = value;
                if (_Encrypt != null)
                    _Encrypt.Alphabet = value;
            }
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> op)
        {
            //how to deal with ignoring the op?
            byte[] keyHash = key.GetMD5Hash();
            var rand = keyHash.KeyToRand();

            InitializeBijection(rand);
            return _Encrypt.Encrypt(plain, key, Forward);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> op)
        {
            //how to deal with ignoring the op?
            byte[] keyHash = key.GetMD5Hash();
            var rand = keyHash.KeyToRand();

            InitializeBijection(rand);
            return _Encrypt.Decrypt(cypher, key, Reverse);
        }

        public byte[] Encrypt(byte[] plain, byte[] key)
        {
            return Encrypt(plain, key, Operation.Custom);
        }

        public byte[] Decrypt(byte[] cypher, byte[] key)
        {
            return Decrypt(cypher, key, Operation.ReverseCustom);
        }

        public byte[] Encrypt(byte[] plain, byte[] key, Operation op = Operation.Custom)
        {
            if (op != Operation.Custom && op!= Operation.ReverseCustom)
                throw new ArgumentException("Bijection Decorator defines its own operation, the only op allowed is custom.", "op");

            return Encrypt(plain, key, null);
        }

        public byte[] Decrypt(byte[] cypher, byte[] key, Operation op = Operation.ReverseCustom)
        {
            if (op != Operation.Custom && op != Operation.ReverseCustom)
                throw new ArgumentException("Bijection Decorator defines its own operation, the only op allowed is custom.", "op");

            return Decrypt(cypher, key, null);
        }

        protected virtual void InitializeBijection(IRng rand)
        {
            int size = Alphabet.Length;
            _Bijection = new List<Map<int, int>>(size);
            for (var ii = 0; ii < size; ++ii)
            {
                _Bijection.Add(new Map<int, int>());
            }

            for (int ii = 0; ii < _Bijection.Count; ++ii)
            {
                List<int> deck = Enumerable.Range(0, _Bijection.Count).ToList();
                deck.Shuffle(rand);
                for (var jj = 0; jj < _Bijection.Count; ++jj)
                    _Bijection[ii].Add(jj, deck[jj]);
            }
        }

        public bool ModulateKey { get; set; }//we might never not want this

        private int Forward(int plain, int key)
        {
            if (ModulateKey)
                key = key.UMod(_Bijection.Count);
            return _Bijection[key].Forward[plain];
        }

        private int Reverse(int cypher, int key)
        {
            if (ModulateKey)
                key = key.UMod(_Bijection.Count);
            return _Bijection[key].Reverse[cypher];
        }

        public string GetBijectionString(string key)
        {
            byte[] keyHash = key.ToByteArray().GetMD5Hash();
            IRng rand = keyHash.KeyToRand();
            InitializeBijection(rand);
            var res = new StringBuilder();

            var columnHeaderList = new List<char>() {' '};
            columnHeaderList.AddRange(Alphabet);
            string columnHeader = string.Join("|", columnHeaderList.Select(c => $" {c} "));
            res.AppendLine(columnHeader);

            int[] alphaIndexes = Alphabet
                .Select(c => (byte)c)
                .Select(b => _Map.Reverse[b])
                .ToArray();

            foreach (int k in alphaIndexes)
            {
                var rowList = new List<char>() { ((char)_Map.Forward[k]) };
                rowList.AddRange(
                    alphaIndexes
                        .Select(ix => Forward(ix, k))
                        .Select(ix => (char)_Map.Forward[ix]));
                string row = string.Join("|", rowList.Select(c => $" {c} "));
                res.AppendLine(row);
            }

            return res.ToString();
        }
    }
}
