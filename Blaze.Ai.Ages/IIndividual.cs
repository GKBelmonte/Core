using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Ai.Ages
{
    public interface IIndividual
    {
        IIndividual Mutate(float probability, float sigma);
        string Name { get; }
    }

    public static class IndividualTools
    {
        public static string CreateName()
        {
            var syllabels = new string[]
            {
                "sel",          "er",           "a",            "fed",          "ed",
                "hi",           "es",           "re",           "hel",          "in",
                "re",           "con",          "sy",           "ter",          "kha",
                "al",           "de",           "com",          "o",            "din",
                "en",           "an",           "tir",          "pin",          "tru",
                "fre",          "de",           "ma",           "kog"
            };

            var result = "";
            for (var ii = 0; ii < Utils.RandomInt(2, 5); ++ii)
            {
                result += syllabels[Utils.RandomInt(0, syllabels.Length)];
            }

            result = result.Substring(0, 1).ToUpper() + result.Substring(1, result.Length - 1);
            return result;
        }
    }
}
