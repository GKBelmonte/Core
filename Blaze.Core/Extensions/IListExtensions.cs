using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Extensions
{
    public static class IListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var r = new Random();
            Shuffle(list, r);
        }

        public static T PickRandom<T>(this IReadOnlyList<T> list, Random rng = null)
        {
            if (rng == null)
                rng = new Random();
            return list[rng.Next(0, list.Count)];
        }

        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list, T1 a, T2 b)
        {
            list.Add(new Tuple<T1,T2>(a, b));
        }

        public static void Add<T1, T2, T3>(this IList<Tuple<T1, T2, T3>> list, T1 a, T2 b, T3 c)
        {
            list.Add(new Tuple<T1, T2, T3>(a, b, c));
        }
    }

    public class ListConstruct<T> : List<T>
    {
        public void Add(params object[] a)
        {
            var t = typeof(T);
            ConstructorInfo c = null;
            if (a.All(b => b != null))
            {
                c = t.GetConstructor(a.Select(b => b.GetType()).ToArray());
            }

            if (c == null)
            {
                var cs = t.GetConstructors();
                c = cs.FirstOrDefault(p => p.GetParameters().Length == a.Length);
            }

            if (c == null)
                throw new InvalidOperationException(string.Format("'{0}' does not contain a constructor that takes the types or number used in 'Add'.", t.Name));

            T ob = (T)c.Invoke(a);
            base.Add(ob);
        }
    }
}
