using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Log
{
    public interface ILogger
    {
        int IndentSize { get; set; }

        void Fatal(object message);

        void Error(object message);

        void Warn(object message);

        void Info(object message);

        void Trace(object message);

        void NewLine();

        IDisposable StartIndentScope();
    }
}
