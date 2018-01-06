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
                        var returnType = TypeToString(method.ReturnType);

                        if (returnType == "System.Void")
                            returnType = "void";

                        var parms = string.Join(",", method.GetParameters().Skip(1).Select(p => $"{ParamIfSet(p)}{TypeToString(p.ParameterType)} {p.Name}"));
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

        private string ParamIfSet(ParameterInfo info)
        {
            return info.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0 ? "params " : "";
        }

        private string TypeToString(Type type)
        {
            if (type.IsGenericType)
            {
                return type.Namespace + "." + type.Name.Substring(0, type.Name.Length - 2) + "<" +
                       string.Join(",", type.GenericTypeArguments.Select(TypeToString)) + ">";
            }

            return type.Namespace + "." + type.Name;
        }

        public IEnumerable<Assembly> References => new Assembly[] {};
        public IEnumerable<string> Imports => new[] { "System" };
        public int Priority => 100;
    }
}
