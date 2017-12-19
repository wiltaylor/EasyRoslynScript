using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyRoslynScript
{
    public class ExtensionMethodHandler : IScriptPreCompileHandler
    {
        public string Process(string script, string folder)
        {
            var sb = new StringBuilder(script);

            sb.AppendLine();
            sb.AppendLine("/***************************************************************************************");
            sb.AppendLine("* Start Exension method alias functions...");
            sb.AppendLine("****************************************************************************************/");
            sb.AppendLine();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var method in from t in asm.GetTypes()
                        where t.IsSealed && !t.IsGenericType && !t.IsNested
                        from m in t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        where m.IsDefined(typeof(ExtensionAttribute), false)
                        where m.GetParameters()[0].ParameterType == typeof(IScriptContext)
                        select m)
                    {

                        var returnType = method.ReturnType.Name;

                        if (returnType == "Void")
                            returnType = "void";

                        var parms = string.Join(",", method.GetParameters().Skip(1).Select(p => $"{p.ParameterType.Name} {p.Name}"));
                        var firstParmName = method.GetParameters()[0].Name;
                        var passParms = string.Join(",", method.GetParameters().Select(p => $"{p.Name}"));
                       
                        sb.AppendLine($"{returnType} {method.Name} ({parms})");
                        sb.AppendLine("{");

                        sb.AppendLine($"EasyRoslynScript.IScriptContext {firstParmName} = null;");

                        if (returnType != "void")
                            sb.Append("return ");
                        
                        sb.AppendLine($"{method.DeclaringType}.{method.Name}({passParms});");
                        sb.AppendLine("}");
                    }
                }
                catch
                {
                    //Some assmblies don't like being scanned. So skip em.
                }
            }

            sb.AppendLine();
            sb.AppendLine("/***************************************************************************************");
            sb.AppendLine("* End Exension method alias functions...");
            sb.AppendLine("****************************************************************************************/");
            sb.AppendLine();

            return sb.ToString();
        }

        public IEnumerable<Assembly> References => new Assembly[] {};
        public IEnumerable<string> Imports => new[] { "System" };
        public int Priority => 100;
    }
}
