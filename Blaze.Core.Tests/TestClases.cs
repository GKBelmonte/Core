using Blaze.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Tests
{
    public class XYPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public static bool operator ==(XYPoint l, XYPoint r)
        {
            if (ReferenceEquals(l, r))
                return true;
            if (ReferenceEquals(null, l) || ReferenceEquals(null, r))
                return false;
            return l.X == r.X && l.Y == r.Y;
        }

        public static bool operator !=(XYPoint l, XYPoint r)
        {
            return !(l == r);
        }

        public override bool Equals(object obj)
        {
            var casted = obj as XYPoint;
            return this == casted;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class Alpha
    {
        public int One { get; set; }
        public List<string> Two { get; set; }
        public double Three { get; set; }
        public static bool operator ==(Alpha l, Alpha r)
        {
            if (ReferenceEquals(l, r))
                return true;
            if (ReferenceEquals(null, l) || ReferenceEquals(null, r))
                return false;
            return l.One == r.One
                && l.Two.SequenceEqualWithNull(r.Two)
                && l.Three == r.Three;
        }

        public static bool operator !=(Alpha l, Alpha r)
        {
            return !(l == r);
        }

        public override bool Equals(object obj)
        {
            var casted = obj as Alpha;
            return this == casted;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class Bravo
    {
        public string First { get; set; }
        public int[] Second { get; set; }
        public static bool operator ==(Bravo l, Bravo r)
        {
            if (ReferenceEquals(l, r))
                return true;
            if (ReferenceEquals(null, l) || ReferenceEquals(null, r))
                return false;
            return l.First == r.First
                && l.Second.SequenceEqualWithNull(r.Second);
        }

        public static bool operator !=(Bravo l, Bravo r)
        {
            return !(l == r);
        }

        public override bool Equals(object obj)
        {
            var casted = obj as Bravo;
            return this == casted;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class Alpha2 : Alpha
    {
        public Bravo Four { get; set; }
        public static bool operator ==(Alpha2 l, Alpha2 r)
        {
            if (ReferenceEquals(l, r))
                return true;
            if (ReferenceEquals(null, l) || ReferenceEquals(null, r))
                return false;
            return (Alpha)l == (Alpha)r 
                && l.Four == r.Four;
        }

        public static bool operator !=(Alpha2 l, Alpha2 r)
        {
            return !(l == r);
        }

        public override bool Equals(object obj)
        {
            var casted = obj as Alpha2;
            return this == casted;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
