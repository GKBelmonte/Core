using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Math;
using Blaze.Core.Extensions;

namespace Encryption
{
    public static class EncryptionTesting
    {
        private static Random _RNG;
        //Bit flipping by 1 in key should flip 50% of cypher text
        /// <summary>
        /// Measure of how much 1 bit in the key changes the cypher text.
        /// </summary>
        /// <returns> A score from 0 to 1</returns>
        public static float TestForConfusion(IEncrypt encrypt, int testCount)
        {
            byte[] plain;
            byte[] key;
            byte[] originalCypher;
            InitTest(encrypt, out plain, out key, out originalCypher);

            List<byte[]> keyFlips = CreateFlips(key, testCount);
            List<byte[]> cypherFlips = keyFlips.Select(k => encrypt.Encrypt(plain, k)).ToList();

            var scores = new List<float>();
            foreach (var cypherFlip in cypherFlips)
            {
                scores.Add(PercentFlips(originalCypher, cypherFlip));
            }
            float ave = scores.Average();
            Debug.Assert(ave >= 0f && ave <= 1f, "Percentage is not within 0 and 1!");
            //The further from 50%, the worse.
            return (0.5f - Math.Abs(0.5f - ave)) / 0.5f;
        }

        /// <summary>
        /// Diffusion is a measure of how each bit of the plain text affects the bits of the cypher text.
        /// </summary>
        public static float TestForDifussion(IEncrypt encrypt, int testCount)
        {
            byte[] plain;
            byte[] key;
            byte[] originalCypher;
            InitTest(encrypt, out plain, out key, out originalCypher);

            List<byte[]> plainFlips = CreateFlips(plain, testCount);
            List<byte[]> cypherFlips = plainFlips.Select(pf => encrypt.Encrypt(pf, key)).ToList();

            var scores = new List<float>();
            foreach (var cypherFlip in cypherFlips)
            {
                scores.Add(PercentFlips(originalCypher, cypherFlip));
            }
            float ave = scores.Average();
            Debug.Assert(ave >= 0f && ave <= 1f, "Percentage is not within 0 and 1!");
            //The further from 50%, the worse.
            return (0.5f - Math.Abs(0.5f - ave)) / 0.5f;
        }

        private static void InitTest(IEncrypt encrypt, out byte[] plain, out byte[] key, out byte[] originalCypher)
        {
            _RNG = new Random(RSeed);

            plain = RandomText(PlainSize, UseNatural);
            key = RandomText(KeySize);
            originalCypher = encrypt.Encrypt(plain, key);
        }

        /// <summary>
        /// Calculates the percentage of bits that were flipped from source
        /// to target. 
        /// 0% flips means they are identical
        /// 100% flips means that every bit got flipped.
        /// </summary>
        public static float PercentFlips(byte[] original, byte[] processed)
        {
            int accumulatedFlips = 0;

            for (var ii = 0; ii < original.Length && ii < processed.Length; ++ii)
            {
                byte fliped = (byte)(original[ii] ^ processed[ii]);
                accumulatedFlips += fliped.CountBits();
            }

            float totalBits = (8f * Math.Min(original.Length, processed.Length));
            float res = ((float)accumulatedFlips) / totalBits;
            Debug.Assert(res >= 0f && res <= 1f, "Percentage is not within 0 and 1!");
            return res;
        }

        private static List<byte[]> CreateFlips(byte[] buff, int num)
        {
            var res = new List<byte[]>();
            for (var ii = 0; ii < num; ++ii)
            {
                byte[] flip = new byte[buff.Length];
                buff.CopyTo(flip, 0);
                int flipByte = _RNG.Next(buff.Length);
                int flipBit = _RNG.Next(8);
                flip[flipByte] ^= (byte)(1 << flipBit);
                res.Add(flip);
            }
            return res;
        }

        public static byte[] RandomText(int size, bool useNatural = false)
        {
            if (useNatural)
            {
                return LoremIpsum[_RNG.Next(LoremIpsum.Length)].ToByteArray();
            }
            else
            {
                var res = new byte[size];
                _RNG.NextBytes(res);
                return res;
            }
        }


        /// <summary>
        /// The distribution should be as spread as possible
        /// </summary>
        public static float TestForDistribution(IEncrypt enc)
        {
            byte[] plain;
            byte[] key;
            byte[] cypher;
            _RNG = new Random(RSeed);
            float[] scores = new float[TryCount];
            for (int ii = 0; ii < TryCount; ++ii)
            {
                plain = RandomText(PlainSize, UseNatural);
                key = RandomText(KeySize);
                cypher = enc.Encrypt(plain, key);

                double plainChi = Math.Sqrt(Stats.ChiSquared(GetBlockCout(plain)));
                double cypherChi = Math.Sqrt(Stats.ChiSquared(GetBlockCout(cypher)));

                //if cypherChi == 0, score is 100% (uniform distribution, little statistical data)
                //if cypherChi == plainChi, score is 0% (same distribution as plain-text, not good at all)
                //if cypherChi > plainChi, score is negative (cypher has worse distribution than plain, probably inconclusive)
                scores[ii] = (float)((plainChi - cypherChi) / plainChi);
            }

            return scores.Average();
        }


        public static List<int> GetBlockCout(byte[] buff, int blockSize = 1, byte[] alphabet = null)
        {
            if (alphabet == null)
                alphabet = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

            var d = new Dictionary<byte, int>();
            foreach (byte b in alphabet)
                d.Add(b, 0);

            foreach (byte b in buff)
                d[b] += 1;

            return d.Values.ToList();
        }

        public static int RSeed = 0;
        public static int PlainSize = 1024;
        public static int KeySize = 8;
        public static int TryCount = 10;
        public static bool UseNatural = false;

        public static string[] LoremIpsum = {
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec in dolor felis. Duis tristique metus id nibh feugiat finibus. Vestibulum vitae viverra elit. Integer iaculis eleifend elementum. Curabitur faucibus commodo nisl imperdiet lobortis. Phasellus pretium mollis purus id pharetra. Integer aliquet nibh velit, et luctus diam malesuada nec. Donec fringilla sem id eros efficitur, non tincidunt risus gravida. Donec mattis sapien lorem, ac malesuada sapien ornare sed. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi aliquam vitae libero vitae elementum. Aenean tellus enim, pretium ac lacus nec, iaculis fringilla quam. In at tellus eu felis malesuada hendrerit. Maecenas efficitur hendrerit leo, eget vulputate odio interdum et. Aliquam in justo fermentum, sollicitudin tellus nec, sagittis lacus. Maecenas pulvinar vitae nibh in maximus. In suscipit tortor tortor, vel sagittis erat vulputate ac. Aenean consequat velit eget est faucibus, ac feugiat leo tristique. Sed non diam nulla. Nullam massa nunc. ",
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sit amet facilisis sem, vel molestie enim. Phasellus tortor libero, eleifend a neque et, tincidunt viverra nisi. Cras nec mattis dolor. Donec suscipit in nibh ac eleifend. Proin fermentum nulla leo, at varius massa euismod vel. Aliquam tortor ante, viverra in ex id, congue cursus purus. Aliquam consequat nibh sit amet ante accumsan sollicitudin vitae id erat. Maecenas vitae purus tempor, laoreet lectus eu, sagittis lacus. Donec tellus ipsum, viverra id ullamcorper et, tincidunt at purus. Fusce sagittis purus non ipsum suscipit, eget interdum mauris laoreet. Sed hendrerit facilisis eros ac hendrerit. Proin consectetur ipsum id dignissim pretium. Donec ut eros dui. Nunc a eros sed massa porta auctor sed non enim. Curabitur lobortis odio lacus, ac cursus velit congue sit amet. Quisque sagittis nunc a diam scelerisque auctor. Vivamus aliquam, ante et bibendum consectetur, sem massa consequat justo, at viverra nisl nibh vel massa. Praesent interdum sed.",
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed semper est id dolor viverra ultrices. Interdum et malesuada fames ac ante ipsum primis in faucibus. Nullam purus massa, suscipit ut vestibulum eget, faucibus non quam. Phasellus bibendum tincidunt lacinia. Nam et accumsan neque. Suspendisse gravida pretium nisl eu porta. Proin metus nunc, pharetra quis porttitor ac, elementum a nulla. Donec ut ullamcorper justo, in vulputate quam. Proin varius eleifend lorem. Aenean consequat velit neque, eget consequat ligula semper vel. Vestibulum placerat nulla non sollicitudin porta. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nam iaculis nisi ac dapibus scelerisque. Integer non eros lacus. Donec odio velit, lobortis vel rutrum in, congue in justo. Aenean quis elit ac felis lacinia imperdiet. Nulla mi est, elementum nec semper id, suscipit vel nisl. Fusce iaculis dolor egestas, convallis massa nec, pellentesque erat. Aliquam enim massa, ornare eget commodo vel metus.",
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque nec imperdiet nisl, vel tincidunt nisl. Ut sed ex eu dui elementum gravida. Nulla suscipit accumsan leo, blandit pulvinar elit imperdiet at. Maecenas rutrum placerat consequat. Suspendisse consequat nisi ornare quam hendrerit, vitae efficitur risus varius. Phasellus in laoreet elit, quis maximus est. Maecenas ullamcorper vel lorem vel varius. Cras tempus ipsum eget quam rhoncus pharetra. Proin mauris arcu, placerat porttitor orci lobortis, rhoncus consequat nisi. In at rutrum tellus, et scelerisque magna. Aliquam nisi urna, euismod eu justo sit amet, malesuada gravida dolor. Donec id tincidunt nisl. Pellentesque luctus placerat sem at hendrerit. Etiam ac leo nibh. Nunc quis ante scelerisque, tincidunt lorem nec, placerat neque. Sed tristique nisi ac magna auctor, vel eleifend neque auctor. Etiam tempus semper rutrum. Cras facilisis pharetra sem ac sagittis. Vivamus dictum ligula vel nisi lacinia, non convallis sapien imperdiet. Cras viverra amet.",
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse pharetra mi a consectetur vehicula. Ut et est ac est porttitor accumsan vel non felis. Curabitur ut massa ut odio accumsan congue. Quisque dignissim blandit metus, sit amet gravida dolor fringilla laoreet. Proin dapibus sed mauris sit amet semper. Donec quis libero vitae ipsum sodales auctor. Vivamus a dolor sit amet diam dignissim suscipit quis ut enim. Donec in tortor a mi finibus pretium. Aenean et leo tortor. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Maecenas pretium, nisi eu cursus tempor, ex metus vehicula nisi, et tincidunt nibh enim et diam. Nunc imperdiet mollis dolor id scelerisque. Nullam elit metus, dapibus tempus urna non, vehicula pulvinar turpis. Nulla ut elit eleifend, vehicula ante ut, porta justo. Cras sit amet erat semper, volutpat lectus in, euismod massa. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec a augue in odio congue ullamcorper. Sed elementum sed.",
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque posuere, risus sed bibendum interdum, elit purus sollicitudin risus, quis elementum nisi diam quis enim. Ut ut orci sed mi rhoncus scelerisque ut ut tellus. Vivamus ut faucibus nulla. Quisque sed accumsan ipsum, a dictum ligula. Praesent semper gravida lacus, vel finibus elit malesuada id. Donec massa augue, tempor id sagittis finibus, maximus sed sem. Vestibulum ac ultricies sem. Phasellus turpis metus, maximus eget ultricies at, vulputate et nunc. Pellentesque ornare odio ut ipsum pretium, quis mollis est volutpat. Ut ut fermentum diam. Nullam elementum volutpat tincidunt. Nunc orci augue, porta id rutrum ac, dignissim eget nulla. Etiam consequat finibus ex nec porta. Mauris iaculis, ante eget efficitur tincidunt, urna eros viverra eros, eget venenatis arcu velit id augue. Sed non elit in libero accumsan luctus. Etiam posuere elit odio. Fusce iaculis dolor bibendum sapien tempor, vitae imperdiet turpis sagittis. Sed aliquam nisl purus volutpat.",
@"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis ullamcorper eros ut ante vehicula pretium. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Vivamus pharetra gravida augue vitae scelerisque. Nunc non rhoncus diam. Ut a pellentesque erat. Curabitur sit amet fringilla lorem, sit amet rhoncus mi. Suspendisse placerat mi velit, ac pretium nunc fringilla eget. Maecenas semper massa ullamcorper est tempus mattis. Nunc at ullamcorper mauris, ut pulvinar ipsum. Cras a commodo lorem. Donec elementum turpis in tellus aliquet, sed egestas nibh vestibulum. Quisque elementum, diam ut pulvinar commodo, urna tortor condimentum sapien, sit amet gravida diam leo id sapien. Morbi interdum vitae dolor id rhoncus. Proin rutrum euismod tempus. Fusce sit amet dignissim ligula. Integer ac suscipit orci. Cras pellentesque tortor sed mollis porta. Vestibulum nec ipsum id ipsum euismod sollicitudin vel in nibh. Donec bibendum lectus lorem, nec pharetra nulla laoreet et volutpat."
        };
    }
}
