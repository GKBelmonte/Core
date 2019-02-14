using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Ai.Ages
{
    public interface IIndividual
    {
        /// <summary>
        /// Creates a new individual that is a mutated version of the old individual
        /// </summary>
        /// <param name="r">Should be used to have deterministic runs</param>
        IIndividual Mutate(float probability, float sigma, Random r);
        string Name { get; }
    }

    public static class IndividualTools
    {
        public static bool NameIndividuals { get; set; }

        public static string CreateName(Random r = null)
        {
            if (!NameIndividuals)
                return string.Empty;

            r = r ?? Utils.ThreadRandom;
            var syllabels = new string[]
            {
                "sel",          "er",           "a",            "fed",          "ed",
                "hi",           "es",           "re",           "hel",          "in",
                "re",           "con",          "sy",           "ter",          "kha",
                "al",           "de",           "com",          "o",            "din",
                "en",           "an",           "tir",          "pin",          "tru",
                "fre",          "de",           "ma",           "kog",          "fus",
                "roh",          "dah"
            };

            var result = "";
            for (var ii = 0; ii < r.Next(2, 5); ++ii)
            {
                result += syllabels[r.Next(0, syllabels.Length)];
            }

            result = result.Substring(0, 1).ToUpper() + result.Substring(1, result.Length - 1);
            return result;
        }
    }
}
