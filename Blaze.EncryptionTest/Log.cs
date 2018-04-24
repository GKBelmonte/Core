using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;
using Blaze.Core.Log;

namespace Blaze.Cryptography.Tests
{
    public class TestLogger : ConsoleLogger
    {
        private int _depth;
        public TestLogger(int depth = 100)
        {
            IndentSize = 2;
            _depth = depth;
        }

        protected override void Log(object message)
        {
            if (CurrentIndent > _depth)
                return;
            base.Log(message);
            Debug.WriteLine(GetStringMessage(message));
        }
    }
}
