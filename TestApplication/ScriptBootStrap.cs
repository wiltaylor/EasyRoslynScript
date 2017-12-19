using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyRoslynScript;

namespace TestApplication
{
    public class ScriptBootStrap : IScriptPreCompileHandler
    {
        public string Process(string script, string folder)
        {
            return script;
        }

        public IEnumerable<Assembly> References => AppDomain.CurrentDomain.GetAssemblies();
        public IEnumerable<string> Imports => new [] {"System"};
        public int Priority => 1;
    }
}
