using Blaze.Core.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography.Classics
{
    // Silly cypher as implemented from Wikipedia.
    public class AutokeyCypher : AlphabeticCypher
    {
        protected Array2D<int> _tabulaRecta;
        public AutokeyCypher() { }

        private void InitRabulaRectaInternal()
        {
            if (_tabulaRecta != null)
                return;
            InitTabulaRecta();
        }

        protected virtual void InitTabulaRecta()
        {
            InitClassicTabulaRecta();
        }

        private void InitClassicTabulaRecta()
        {
            int size = Alphabet.Count;
            _tabulaRecta = new Array2D<int>(size, size);
            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    _tabulaRecta[i, j] = (i + j) % size;
        }

        public override IReadOnlyList<char> Alphabet
        {
            get => base.Alphabet;
            set
            {
                base.Alphabet = value;
                _tabulaRecta = null;
                InitRabulaRectaInternal();
            }
        }

        public override byte[] Encrypt(byte[] plain, byte[] key, Func<int, int, int> op)
        {
            InitRabulaRectaInternal();
            int[] plainIx = ByteToIndices(plain);
            int[] keyIx = ByteToIndices(key);
            int[] cypherIx = new int[plain.Length];

            int i;
            for (i = 0; i < keyIx.Length; ++i)
                cypherIx[i] = _tabulaRecta[keyIx[i], plainIx[i]];

            for(int j = i; j < plainIx.Length; ++j)
                cypherIx[j] = _tabulaRecta[plainIx[j-i], plainIx[j]];

            return IndicesToBytes(cypherIx);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key, Func<int, int, int> op)
        {
            InitRabulaRectaInternal();
            int[] cypherIx = ByteToIndices(cypher);
            int[] keyIx = ByteToIndices(key);
            int[] plainIx = new int[cypher.Length];

            int i;
            for (i = 0; i < keyIx.Length; ++i)
            {
                int c;
                for (c = 0; c < _tabulaRecta.ColumnCount; ++c)
                    if (_tabulaRecta[keyIx[i], c] == cypherIx[i])
                        break;
                plainIx[i] = c;
            }

            for (int j = i; j < cypherIx.Length; ++j)
            {
                int c;
                for (c = 0; c < _tabulaRecta.ColumnCount; ++c)
                    if (_tabulaRecta[plainIx[j-i], c] == cypherIx[j])
                        break;
                plainIx[j] = c;
            }

            return IndicesToBytes(plainIx);
        }
    }
}
