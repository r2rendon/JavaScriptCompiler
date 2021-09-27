using JavaScriptCompiler.Lexer;
using System;
using System.IO;

namespace JavaScriptCompiler.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = File.ReadAllText("code.txt").Replace(Environment.NewLine, "\n");
            var input = new Input(code);
            var scanner = new Scanner(input);
            var parser = new Parser.Parser(scanner);
            parser.Parse();
            System.Console.WriteLine("Success!{code}");
        }
    }
}
