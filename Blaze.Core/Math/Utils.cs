using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Math
{
    public class Utils
    {
        public static bool IsPerfectSquare(int num)
        {
            int sqrt = (int)System.Math.Sqrt(num);
            return sqrt * sqrt == num;
        }
    }
}
