using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Core.Log
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger() : this(null) { }

        public ConsoleLogger(Type t)
        {
            _IndentSize = 4;
            _IndentStr = string.Empty;
            _Type = t;
        }

        public int IndentSize
        {
            get { return _IndentSize; }
            set
            {
                _IndentSize = value;
                UpdateIndentStr();
            }
        }

        public void Fatal(object message)
        {
            using (new ColorScope(ConsoleColor.Red, ConsoleColor.White))
                Log(message);
        }

        public void Error(object message)
        {
            using (new ColorScope(ConsoleColor.Red))
                Log(message);
        }

        public void Warn(object message)
        {
            using (new ColorScope(ConsoleColor.Yellow))
                Log(message);
        }

        public void Info(object message)
        {
            using (new ColorScope(ConsoleColor.White))
                Log(message);
        }

        public void Trace(object message)
        {
            using (new ColorScope(ConsoleColor.DarkGray))
                Log(message);
        }

        public IDisposable StartIndentScope() { return new Scope(this); }

        private int _IndentSize;
        private int _Indent;
        private string _IndentStr;
        private Type _Type;

        private void Log(object message)
        {
            Console.Write(_IndentStr);
            Console.WriteLine(message);
        }

        private void UpdateIndentStr()
        {
            _IndentStr = string.Join(string.Empty,
                Enumerable
                .Range(0, _Indent * IndentSize)
                .Select(i => " "));
        }

        private class Scope : IDisposable
        {
            private ConsoleLogger _logger;
            public Scope(ConsoleLogger logger)
            {
                _logger = logger;
                _logger._Indent += 1;
                _logger.UpdateIndentStr();
            }

            public void Dispose()
            {
                _logger._Indent -= 1;
                _logger.UpdateIndentStr();
            }
        }

        private class ColorScope : IDisposable
        {
            ConsoleColor _originalColor;
            ConsoleColor _originalBackground;
            public ColorScope(ConsoleColor color, ConsoleColor? background = null)
            {
                _originalColor = Console.ForegroundColor;
                _originalBackground = Console.BackgroundColor;
                Console.ForegroundColor = color;
                if (background.HasValue)
                    Console.BackgroundColor = background.Value;
            }

            public void Dispose()
            {
                Console.ForegroundColor = _originalColor;
                Console.BackgroundColor = _originalBackground;
            }
        }
    }
}
