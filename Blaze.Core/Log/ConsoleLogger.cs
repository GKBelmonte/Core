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
            _indentSize = 4;
            _indentStr = string.Empty;
            _type = t;
        }

        public int IndentSize
        {
            get { return _indentSize; }
            set
            {
                _indentSize = value;
                UpdateIndentStr();
            }
        }

        public int CurrentIndent { get; private set; }

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

        public void NewLine()
        {
            Log(string.Empty);
        }

        public IDisposable StartIndentScope() { return new Scope(this); }

        private int _indentSize;
        private string _indentStr;
        private Type _type;

        protected virtual void Log(object message)
        {
            Console.WriteLine(GetStringMessage(message));
        }

        protected virtual string GetStringMessage(object message)
        {
            return _indentStr + message;
        }

        private void UpdateIndentStr()
        {
            _indentStr = string.Join(string.Empty,
                Enumerable
                .Range(0, CurrentIndent * IndentSize)
                .Select(i => " "));
        }

        protected class Scope : IDisposable
        {
            private ConsoleLogger _logger;
            public Scope(ConsoleLogger logger)
            {
                _logger = logger;
                _logger.CurrentIndent += 1;
                _logger.UpdateIndentStr();
            }

            public void Dispose()
            {
                _logger.CurrentIndent -= 1;
                _logger.UpdateIndentStr();
            }
        }

        protected class ColorScope : IDisposable
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
