using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AiLibrary
{
    public interface IReadableState
    {
        string StateMove();
        string State();
        string State(int linePadding);
    }
}
