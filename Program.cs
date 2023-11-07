using GalgameNovelScript;
using System;
using System.Collections.Generic;
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
            Console.WriteLine(tree);
        }
    }
}

