using GalgameNovelScript;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using static System.Net.Mime.MediaTypeNames;

namespace GalgameNovelScriptTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            string text;
            using (var fs = File.Open(args[0], FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fs))
                text = sr.ReadToEnd();
            var lexer = new Lexer(text);
            var parser = new Parser(lexer);
            var tree = parser.Parse();
            var json = JsonConvert.SerializeObject(tree, Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto
                });
            using (var fs = File.Open("ast.json", FileMode.Create, FileAccess.Write))
                fs.Write(System.Text.Encoding.UTF8.GetBytes(json));
            Console.WriteLine("语法解析完成！");

            //var interpreter = new Interpreter(tree);
            //interpreter.AddToGlobalScope("输入", new Func<string>(Console.ReadLine));
            //interpreter.AddToGlobalScope("输出", new Action<object>(Console.WriteLine));
            //interpreter.Interpret();
        }
    }
}
