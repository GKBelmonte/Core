using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Maths
{
    public static class Utils
    {
        public static bool IsPerfectSquare(int num)
        {
            int sqrt = (int)System.Math.Sqrt(num);
            return sqrt * sqrt == num;
        }

        public static double Clamp(this double self, double bot, double top)
        {
            if (self > top)
                return top;
            if (self < bot)
                return bot;
            return self;
        }

        //public static double ToSignificantDigits(this float self, int digits)
        //{
        //}

        //public static float ToSignificantDigits(this float self, int digits)
        //{
        //}
    }
}
