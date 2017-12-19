using System.Collections.Generic;
using System.Reflection;

namespace EasyRoslynScript
{
    public interface IScriptPreCompileHandler
    {
        string Process(string script, string folder);
        IEnumerable<Assembly> References { get; }
        IEnumerable<string> Imports { get; }
        int Priority { get; }
    }
}
