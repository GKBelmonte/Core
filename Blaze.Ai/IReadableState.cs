using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Ai
{
    public interface IReadableState
    {
        string StateMove();
        string State();
        string State(int linePadding);
    }
}
