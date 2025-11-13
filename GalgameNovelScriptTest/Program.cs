using GalgameNovelScript;

namespace GalgameNovelScriptTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            using (var fs = File.Open(args[0], FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fs))
            {
                var text = sr.ReadToEnd() + "\n";
                var lexer = new Lexer(text);
                var parser = new Parser(lexer);
                var tree = parser.Parse();
                Console.WriteLine("语法解析完成！");

                //var interpreter = new Interpreter(tree);
                //interpreter.AddToGlobalScope("输入", new Func<string>(Console.ReadLine));
                //interpreter.AddToGlobalScope("输出", new Action<object>(Console.WriteLine));
                //interpreter.Interpret();
                // 暂停
                Console.ReadLine();
            }
        }
    }
}
