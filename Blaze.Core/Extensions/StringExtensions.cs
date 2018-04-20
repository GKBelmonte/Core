using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Extensions
{
    static public class StringExtensions
    {
        //Using default encoding will fail if the alphabet does not contain null if we have a strict alphabet
        // e.g. alphanumerical only alphabet A -> (00 62) ->  KeyNotFound X
        //Using UTF-8 Encoding might fail since converting back to UTF-8 can yield question marks.
        // e.g. A -> UTF8-invalid. -> CantConvertBack
        // We need a don't-give-a-damn-dont-interpret-version of the to and from.
        // Or an input Alphabet and output Alphabet that can be distinct

        public static byte[] ToByteArray(this string str)
        {
            return str.Select(c => (byte)c).ToArray();
        }

        public static string ToTextString(this byte[] bytes)
        {
            return string.Join(string.Empty, bytes.Select(c => (char)c));
        }

        public static byte[] Encode(this string self, Encoding enc = Encoding.ASCII)
        {
            switch (enc)
            {
                case Encoding.ASCII:
                    return System.Text.Encoding.ASCII.GetBytes(self);
                case Encoding.UTF8:
                    return System.Text.Encoding.UTF8.GetBytes(self);
                case Encoding.UTF16:
                    return System.Text.Encoding.Unicode.GetBytes(self);
            }
            Trace.Assert(false);
            return null;
        }

        public static string Decode(this byte[] self, Encoding enc = Encoding.UTF8)
        {
            switch (enc)
            {
                case Encoding.ASCII:
                    return System.Text.Encoding.ASCII.GetString(self);
                case Encoding.UTF8:
                    return System.Text.Encoding.UTF8.GetString(self);
                case Encoding.UTF16:
                    return System.Text.Encoding.Unicode.GetString(self);
            }
            Trace.Assert(false);
            return null;
        }

        public enum Encoding
        {
            ASCII,
            UTF8,
            UTF16
        }
    }
}
