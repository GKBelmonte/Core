using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Collections;
using Blaze.Core.Extensions;
using Blaze.Cryptography.Rng;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Blaze.Cryptography
{
    public class HillCypher : AlphabeticCypher
    {
        /* 
         Shuffle cypher gave me a realisation. All transposition cyphers can be
         described as matrix multiplication
         For example, for plain text PLAIN
         and 2 columns we have:
         PL  => PANLI
         AI
         N

          OR
          1 0 0 0 0     P   P
          0 0 1 0 0     L   A
          0 0 0 0 1 x   A = N
          0 1 0 0 0     I   L
          0 0 0 1 0     N   I

         The pattern would keep repeating if the text was longer.
         Point is that, the other entries need not be null, so long  as the 
         matrix is reversible, we can get the original plain text.
         Much like the bifid cypher, this distributed both the key and the plain
         accross much of the cypher.

         So I googled, and I found Hill's cypher.
        */

        private int _matrixSize;
        private int _matrixDimension;
        public override byte[] Encrypt(byte[] plain, byte[] key)
        {
            //Let's encrypt 8x8 or 16x16 at a time
            //Insert padding amount at the beginning of the cypher so we know how much to remove
            _matrixDimension = 16;
            if (plain.Length < 16 * 16)
                _matrixDimension = 8;
            _matrixSize = _matrixDimension * _matrixDimension;
            //fix this
            List<int> plainIxs = ByteToIndices(plain).ToList();

            //calculate fill
            int fill = plain.Length % _matrixSize;
            var noise = new SysRng();
            for (int i = 0; i < fill; ++i)
                plainIxs.Add(noise.Next(0, Alphabet.Count));

            List<int> cypherIxs = new List<int>(plainIxs.Count + 4);
            cypherIxs.AddRange(EncodeFill(fill, Alphabet.Count));

            IRng rng = key.KeyToRand();
            for (int i = 0; i < plainIxs.Count; i += _matrixDimension)
            {
                cypherIxs.AddRange(Encrypt(new ListSpan<int>(plainIxs, i, i + _matrixDimension), rng));
            }

            return IndicesToBytes(cypherIxs);
        }

        public IEnumerable<int> Encrypt(IReadOnlyList<int> plainIxs, IRng rng)
        {
            DenseMatrix keyStreamMatrix = CreateEncryptionMatrix(rng);

            #if DEBUG
            {
                int det = ((int)Math.Round(keyStreamMatrix.Determinant())).UMod(Alphabet.Count);

                DenseMatrix inv = (DenseMatrix)keyStreamMatrix.Inverse();
                MatrixUMod(inv, Alphabet.Count);

                var identity = keyStreamMatrix * inv;
                MatrixUMod(identity, Alphabet.Count);
                //Debug.Assert(identity.Equals(Matrix.Build.SparseIdentity(_matrixDimension)));
            }
            #endif

            //Column Major ordering is not what I intended but wtv, free transposition
            double[] plainIxsDouble = plainIxs.Select(i => (double)i).ToArray();
            var plainVector = new DenseVector(plainIxsDouble);

            DenseVector cypherVector = keyStreamMatrix * plainVector;
            return VectorToList(cypherVector, Alphabet.Count);
        }

        public override byte[] Decrypt(byte[] cypher, byte[] key)
        {
            int[] cypherIxs = ByteToIndices(cypher);
            int fill = DecodeFill(cypherIxs, Alphabet.Count);

            int originalPlainLength = cypher.Length - 4 - fill;
            //Let's encrypt 8x8 or 16x16 at a time
            //Insert padding amount at the beginning of the cypher so we know how much to remove
            _matrixDimension = 16;
            if (originalPlainLength < 16 * 16)
                _matrixDimension = 8;
            _matrixSize = _matrixDimension * _matrixDimension;

            var plainIxs = new List<int>(cypherIxs.Length);
            IRng rng = key.KeyToRand();
            for (int i = 4; i < cypherIxs.Length; i += _matrixDimension)
            {
                plainIxs.AddRange(Decrypt(new ListSpan<int>(cypherIxs, i, i + _matrixDimension), rng));
            }

            plainIxs.RemoveRange(plainIxs.Count - fill, fill);

            return IndicesToBytes(plainIxs);
        }

        public IEnumerable<int> Decrypt(IReadOnlyList<int> cypherIxs, IRng rng)
        {
            DenseMatrix keyStreamMatrix = CreateEncryptionMatrix(rng);
            // inv = 1/det(A) * (C(A))^T
            // det = 1 and Adj = C(A)^T
            // so adj(A) == inv
            // modinv = det(A) * adj(A) mod m
            // but det(A) == 1
            DenseMatrix inv = (DenseMatrix)keyStreamMatrix.Inverse();
            //Don't think i even need this
            //MatrixUMod(inv, Alphabet.Count);

            double[] cypherIxsDouble = cypherIxs.Select(i => (double)i).ToArray();
            var cypherVector = new DenseVector(cypherIxsDouble);

            var plainIxs = inv * cypherVector;
            return VectorToList(plainIxs, Alphabet.Count);
        }

        private static IEnumerable<int> MatrixToList(Matrix<double> mx, int mod)
        {
            for (int r = 0; r < mx.RowCount; ++r)
                for (int c = 0; c < mx.ColumnCount; ++c)
                    yield return ((int)Math.Round(mx[r, c])).UMod(mod);
        }

        private static IEnumerable<int> VectorToList(DenseVector v, int mod)
        {
            for (int r = 0; r < v.Count; ++r)
                yield return ((int)Math.Round(v[r])).UMod(mod);
        }

        private DenseMatrix CreateEncryptionMatrix(IRng rng)
        {
            var encryptionMatrix = new DenseMatrix(_matrixDimension);
            //Create a random diagonal matrix with determinant 1
            for (int r = 0; r < _matrixDimension; ++r)
            {
                encryptionMatrix[r, r] = 1;
                for (int c = r + 1; c < _matrixDimension; ++c)
                    encryptionMatrix[r, c] = rng.Next(0, Alphabet.Count);
            }

            int[] colPermutation = Enumerable.Range(0, _matrixDimension).Select(i => i).ToArray();
            colPermutation.Shuffle(rng);

            encryptionMatrix.PermuteColumns(new MathNet.Numerics.Permutation(colPermutation));

            if (Math.Abs(encryptionMatrix.Determinant() - 1) > 0.0001)
            {
                int[] fixDeterminant = Enumerable.Range(0, _matrixDimension).Select(i => i).ToArray();
                fixDeterminant[0] = 1;
                fixDeterminant[1] = 0;
                encryptionMatrix.PermuteColumns(new MathNet.Numerics.Permutation(fixDeterminant));
            }

            for (int i = 0; i < _matrixDimension / 4; ++i)
            {
                int sourceRow = rng.Next(0, _matrixDimension);
                int targetRow = rng.Next(0, _matrixDimension);
                if (sourceRow == targetRow)
                    continue;
                int add = rng.Next(0, 2);
                add = add == 1 ? add : -1;
                for (int j = 0; j < _matrixDimension; ++j)
                    encryptionMatrix[targetRow, j] += encryptionMatrix[sourceRow, j] * add;
            }
            MatrixUMod(encryptionMatrix, Alphabet.Count);
            return encryptionMatrix;
        }

        /// <summary>
        /// Encodes the number 'fill' in a base 'numBase' with
        /// Usefull if let's the alphabet size is 32, in which case
        /// fill bigger than 32 cannot be represented in the alphabet.
        /// Biggest possible describable fill will be b^(4) - 1
        /// (could be b^(b-1) - 1 if we prepend the fill-length, but
        /// in practice b > 26, making it useless)
        /// </summary>
        private static List<int> EncodeFill(int fill, int numBase)
        {
            // e.g. base 4, can have up to 4 digits (0-4)
            // the max value being 333 (b4) which is 1000 - 1 (b4)
            // decimal: 4^4 - 1 = 255
            // Our smalles alphabet is 10 (test decimal alphabet), so with 4 digits we have 999
            // should be safe ;)
            int numOfDigits = fill == 0 ? 0 : (int)(Math.Log(fill, numBase) + 1);
            if (numOfDigits > 4)
                throw new InvalidOperationException($"Can't encode {fill} in base {numBase} in more than {numBase - 1}");
            List<int> res = new List<int>(numOfDigits);

            //little endian
            int currentFill = fill;
            for (int i = 0; i < 4; ++i)
            {
                int n = currentFill % numBase;
                currentFill = currentFill / numBase;
                res.Add(n);
            }
            return res;
        }

        private static int DecodeFill(IList<int> header, int numBase)
        {
            int pow = 1;

            //little endian
            int currentFill = 0;
            for (int i = 0; i < 4; ++i)
            {
                currentFill += pow * header[i];
                pow = pow * numBase;
            }
            return currentFill;
        }

        private string MatrixToStr(DenseMatrix m)
        {
            var b = new StringBuilder();
            for (int r = 0; r < m.RowCount; ++r)
            {
                for (int c = 0; c < m.ColumnCount; ++c)
                {
                    b.Append(m[r, c].ToString("0").PadRight(8));
                }
                b.AppendLine();
            }
            return b.ToString();
        }

        private void MatrixUMod(DenseMatrix m, int n)
        {
            for (int r = 0; r < m.RowCount; ++r)
                for (int c = 0; c < m.ColumnCount; ++c)
                    m[r,c] = ((int)Math.Round(m[r, c])).UMod(n);
        }

        protected override byte[] Encrypt(byte[] plain, byte[] key, Op op)
        {
            throw new NotImplementedException();
        }
    }
}
