using Blaze.Cryptography.Rng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    public class Shaker
    {
        IRng Rng { get; set; }

        public virtual byte[] PinchOfSalt(int saltLength)
        {
            byte[] salt = new byte[saltLength];
            if (Rng == null)
            {
                using (var rng = new RNGCryptoServiceProvider())
                    rng.GetNonZeroBytes(salt);
            }
            else
            {
                //Allow mock rng
                Rng.NextBytes(salt);
            }
            return salt;
        }

        public virtual void SaltKey(byte[] key, byte[] salt, out byte[] newKey)
        {
            bool keyIsLonger = key.Length > salt.Length;

            byte[] longSequence, shortSequence;

            if (keyIsLonger)
            {
                longSequence = key;
                shortSequence = salt;
                newKey = new byte[key.Length];
            }
            else
            {
                longSequence = salt;
                shortSequence = key;
                newKey = new byte[salt.Length];
            }

            int i = 0;

            for (; i < shortSequence.Length; ++i)
                newKey[i] = (byte)(shortSequence[i] ^ longSequence[i]);

            for (; i < longSequence.Length; ++i)
                newKey[i] = longSequence[i];
        }
    }
}
