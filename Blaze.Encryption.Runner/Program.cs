using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Plossum.CommandLine;
using Blaze.Core.Log;
using Blaze.Core.Extensions;

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

            ICypher enc = new ChainCypher(
                typeof(Classics.TranspositionCypher),
                typeof(FibonacciCypher), 
                typeof(StreamCypher), 
                typeof(FibonacciCypherV3), 
                typeof(StreamCypher));

            if (ops.PlainText)
                EncryptDecryptPlainText(ops, enc);
            else
                EncryptDecryptBinary(ops, enc);
        }

        private static void EncryptDecryptPlainText(Options ops, ICypher cypher)
        {
            string originalText = File.ReadAllText(ops.SourceFilePath);

            EndOfLine endOfLine;
            if (!AlphabeticCypher.IsTextPlain(originalText, out endOfLine))
                throw new InvalidOperationException("Text is not considered plain text");
            cypher.Alphabet = AlphabeticCypher.GetPlainTextAlphabet(endOfLine);

            if (ops.Action == Action.Encrypt)
            {
                string plainText = originalText;
                string cryptoText = cypher.Encrypt(plainText, ops.EncryptionKey);
                File.WriteAllText(ops.TargetFilePath, cryptoText);
            }
            else
            {
                string cryptoText = originalText;
                string plainText = cypher.Decrypt(cryptoText, ops.EncryptionKey);
                File.WriteAllText(ops.TargetFilePath, plainText);
            }
        }

        private static void EncryptDecryptBinary(Options ops, ICypher cypher)
        {
            byte[] originalText = File.ReadAllBytes(ops.SourceFilePath);

            if (ops.Action == Action.Encrypt)
            {
                byte[] plainText = originalText;
                byte[] cryptoText = cypher.Encrypt(
                    plainText, 
                    ops.EncryptionKey.ToByteArray());
                File.WriteAllBytes(ops.TargetFilePath, cryptoText);
            }
            else
            {
                byte[] cryptoText = originalText;
                byte[] plainText = cypher.Decrypt(
                    cryptoText, 
                    ops.EncryptionKey.ToByteArray());
                File.WriteAllBytes(ops.TargetFilePath, plainText);
            }
        }

        [CommandLineManager(ApplicationName = "Blaze.Crypto.Console", EnabledOptionStyles = OptionStyles.ShortUnix)]
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
