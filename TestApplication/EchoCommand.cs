using System.Collections.Generic;
using System.Reflection;
using System.Text;
using EasyRoslynScript;

namespace TestApplication
{
    public class EchoCommand : IScriptPreCompileHandler
    {
        public string Process(string script, string folder)
        {
            var sb = new StringBuilder(script);
            sb.AppendLine();

            sb.AppendLine("void Echo(string message)");
            sb.AppendLine("{");
            sb.AppendLine(" Console.WriteLine(message);");
            sb.AppendLine("}");

            sb.AppendLine();

            return sb.ToString();
        }

        public IEnumerable<Assembly> References => new Assembly[] { };
        public IEnumerable<string> Imports => new[] {"System"};
        public int Priority => 100;
    }
}
