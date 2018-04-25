using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Plossum.CommandLine;
using Blaze.Core.Log;

namespace Blaze.Cryptography.Runner
{
    class Program
    {
        public enum Action
        {
            Encrypt,
            Decrypt
        }

        static ILogger Log = new ConsoleLogger();

        static void Main(string[] args)
        {
            var ops = new Options();
            using (var parser = new CommandLineParser(ops))
            {
                parser.Parse();
                if (ops.Help)
                {
                    Log.Info(parser.UsageInfo.GetHeaderAsString(Console.WindowWidth));
                    return;
                }
                else if (parser.HasErrors)
                {
                    Environment.ExitCode = 1;
                    return;
                }
            }


            ICypher enc = new ChainCypher(typeof(FibonacciCypher), typeof(StreamCypher), typeof(FibonacciCypherV3));

            EncryptDecryptPlainText(ops, enc);
        }

        private static void EncryptDecryptPlainText(Options ops, ICypher enc)
        {
            string originalText = File.ReadAllText(ops.SourceFilePath);

            if (!AlphabeticCypher.IsTextPlain(originalText))
                throw new InvalidOperationException("Text is not considered plain text");
            enc.Alphabet = AlphabeticCypher.GetPlainTextAlphabet();

            if (ops.Action == Action.Encrypt)
            {
                string plainText = originalText;
                string cryptoText = enc.Encrypt(plainText, ops.EncryptionKey);
                File.WriteAllText(ops.TargetFilePath, cryptoText);
            }
            else
            {
                string cryptoText = originalText;
                string plainText = enc.Decrypt(cryptoText, ops.EncryptionKey);
                File.WriteAllText(ops.TargetFilePath, plainText);
            }
        }

        [CommandLineManager(ApplicationName = "Blaze.Enc", EnabledOptionStyles = OptionStyles.ShortUnix)]
        class Options
        {
            [CommandLineOption]
            public string SourceFilePath { get; set; }

            [CommandLineOption]
            public string TargetFilePath { get; set; }

            [CommandLineOption]
            public string EncryptionKey { get; set; }

            [CommandLineOption]
            public Action Action { get; set; } = Action.Encrypt;

            [CommandLineOption]
            public bool PlainText { get; set; }

            [CommandLineOption]
            public bool Help { get; set; }
        }
    }
}
