using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;

namespace Blaze.Encryption.Tests
{
    public static class Log
    {
        public static int TabSize { get; set; }
        public static int Indent { get; set; }

        public static void Info(string str, params object[] pars)
        {
            //we can now re-route it wherever we want
            if (Indent > 0)
                str = Enumerable.Range(0, TabSize * Indent).Select(i => " ").Join() + str;
            Console.WriteLine(str, pars);
        }

        public static void Info()
        {
            Console.WriteLine();
        }

        static Log()
        {
            TabSize = 2;
        }

        public class IndentScope : IDisposable
        {
            public IndentScope()
            {
                Indent += 1;
            }

            public void Dispose()
            {
                Indent -= 1;
            }
        }
    }
}
