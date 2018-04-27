using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Cryptography
{
    [Flags]
    public enum EndOfLine
    {
        None = 0,
        CarriageReturn = 0b01,
        LineFeed = 0b10,
        Mixed = 0b11
    }
}
