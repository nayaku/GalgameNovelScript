using GalgameNovelScript;
using System;
using System.IO;

public class Program
{
    static void Main(string[] args)
    {
        using (var fs = File.Open("main.ns", FileMode.Open))
        using (var sr = new StreamReader(fs))
        {
            var text = sr.ReadToEnd() + "\n";
            var lexer = new Lexer(text);
            var parser = new Parser(lexer);
            var tree = parser.Parse();

            var interpreter = new Interpreter(tree);
            interpreter.GLOBAL_SCOPE["输出"] = new Action<object>(Console.WriteLine);
            interpreter.GLOBAL_SCOPE["输入"] = new Func<string>(Console.ReadLine);
            interpreter.Interpret();
            // 暂停
            Console.ReadLine();
        }
    }
}

