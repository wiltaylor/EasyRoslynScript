﻿using System;
using System.Collections.Generic;
using System.Text;
using EasyRoslynScript;
using EasyNuGet;
using EasyRoslynScript.NuGet;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var loc = new ServiceLocator();
            loc.NuGetFeedUri = "https://www.myget.org/F/fcepacks/api/v3/index.json";
            var search = new PackageSearcher(loc);
            var downloader = new PackageDownloader(loc);

            var nugetpreproc = new NugetPreProcessor(new NuGetProcessorSettings(), downloader, search);

            var runner = new ScriptRunner(new IScriptPreCompileHandler[] { new ScriptBootStrap(), new ExtensionMethodHandler(), nugetpreproc});
            
            
            try
            {
                var script = new StringBuilder();

                //script.AppendLine("#n nuget:?package=FlexibleConfigEngine&version=0.1.0-unstable.2&prerelease");
                script.AppendLine("#n nuget:?file=D:\\Sandpit\\testbucketofdoom\\semver\\2.0.4\\semver.2.0.4.nupkg");
                script.AppendLine("using Semver;");
                script.AppendLine("Echo(Sum(1, 2).ToString());");
                script.AppendLine("var v = SemVersion.Parse(\"1.1.0-rc.1+nightly.2345\");");
                script.AppendLine("Echo(v.ToString());");
                script.AppendLine("Echo(\"Hey {0} {1}\", \"1\", 2);");

                //var builder = new ScriptBuilder();
                //builder.AppendScriptFile("test.csx");
                //runner.ExecuteString(builder.ToString()).Wait();
                runner.ExecuteString(script.ToString()).Wait();

                Console.WriteLine(runner.Script);

                //foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
                //    Console.WriteLine($"{asm.CodeBase} - {asm.FullName}");
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

        public static void Echo(this IScriptContext context, string message, params object[] args)
        {
            Console.WriteLine(message, args);

        }

        public static int Sum(this IScriptContext context, int num1, int num2)
        {
            return num1 + num2;
        }
    }
}
