using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;
using Blaze.Core.Log;

namespace Blaze.Encryption.Tests
{
    public class TestLogger : ConsoleLogger
    {
        public TestLogger()
        {
            IndentSize = 2;
        }

        protected override void Log(object message)
        {
            base.Log(message);
            Debug.WriteLine(GetStringMessage(message));
        }
    }
}
