using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> self, string sep)
        {
            return string.Join(sep, self);
        }

        public static string Join<T>(this IEnumerable<T> self, char sep)
        {
            return string.Join(sep.ToString(), self);
        }

        public static string Join<T>(this IEnumerable<T> self)
        {
            return string.Join(string.Empty, self);
        }

        public static string Join(this IEnumerable<char> self)
        {
            return new string(self.ToArray());
        }

        public static void AddRange<T>(this HashSet<T> self, IEnumerable<T> stuff)
        {
            foreach (T e in stuff)
                self.Add(e);
        }

        public static TEnum MaxElement<TEnum, TComp>(this IEnumerable<TEnum> self, Func<TEnum, TComp> compSel) where TComp : IComparable
        {
            TComp c = compSel(self.First());
            TEnum res = self.First();

            foreach (TEnum ele in self)
            {
                TComp val = compSel(ele);
                if (val.CompareTo(c) > 0)
                {
                    c = val;
                    res = ele;
                }
            }

            return res;
        }

        public static TEnum MinElement<TEnum, TComp>(this IEnumerable<TEnum> self, Func<TEnum, TComp> compSel) where TComp : IComparable
        {
            TComp c = compSel(self.First());
            TEnum res = self.First();

            foreach (TEnum ele in self)
            {
                TComp val = compSel(ele);
                if (val.CompareTo(c) < 0)
                {
                    c = val;
                    res = ele;
                }
            }

            return res;
        }
    }
}
