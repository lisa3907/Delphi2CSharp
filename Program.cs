using Antlr4.Runtime;
using Delphi.Generated;
using System;
using System.IO;
using System.Text;

namespace Delphi
{
    class Program
    {
        static string TranslatePascalToCSharp(string pascal)
        {
            var inputStream = new AntlrInputStream(pascal);
            var speakLexer = new DelphiLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(speakLexer);
            var pascalParser = new DelphiParser(commonTokenStream);

            var listener = new DelphiListener();
            pascalParser.AddParseListener(listener);
            pascalParser.file();

            return listener.Code;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Start...");

            try
            {
                var files = Directory.GetFiles(@"delphi\", "*.pas");

                foreach (var f in files)
                {
                    try
                    {
                        var source = File.ReadAllText(f);
                        var csharp = TranslatePascalToCSharp(source);

                        File.WriteAllLines(Path.Combine(@"csharp", Path.GetFileNameWithoutExtension(f) + ".cs"), new[] { csharp }, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        Console.Write($"[{f}] ");
                        if (ex.InnerException != null)
                            Console.WriteLine(ex.InnerException.Message);
                        else
                            Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Hit enter to exit...");
            Console.ReadLine();
        }
    }
}