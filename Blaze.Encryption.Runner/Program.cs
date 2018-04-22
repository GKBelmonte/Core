using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Plossum.CommandLine;
using Blaze.Core.Log;

namespace Blaze.Encryption.Runner
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

            IEncrypt enc2 = new RandomBijection(new FibonacciCypher());
            string what = enc2.Encrypt("Is this real life?", "Yes");

            IEncrypt enc = new ChainCypher(typeof(FibonacciCypher), typeof(StreamCypher), typeof(FibonacciCypherV3));
            if (ops.Action == Action.Encrypt)
            {
                string plainText = File.ReadAllText(ops.SourceFilePath);
                string cryptoText = enc.Encrypt(plainText, ops.EncryptionKey);
                File.WriteAllText(ops.TargetFilePath, cryptoText);
            } 
            else
            {
                string cryptoText = File.ReadAllText(ops.SourceFilePath);
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
            public bool Help { get; set; }
        }
    }
}
