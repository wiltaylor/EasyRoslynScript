using System;
using System.Collections.Generic;
using EasyRoslynScript;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new ScriptRunner(new IScriptPreCompileHandler[] { new ScriptBootStrap(), new ExtensionMethodHandler()});

            try
            {
                runner.ExecuteString("Echo(Sum(1, 2).ToString());").Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                Console.WriteLine(runner.Script);
                
            }

            Console.Read();
        }
    }

    public static class Foo
    {

        public static IEnumerable<string> GetFoo(this IScriptContext context, Dictionary<string, string> foodic)
        {
            return foodic.Keys;
        }

        public static void Echo(this IScriptContext context, string message)
        {
            Console.WriteLine(message);
            
        }

        public static int Sum(this IScriptContext context, int num1, int num2)
        {
            return num1 + num2;
        }
    }
}
