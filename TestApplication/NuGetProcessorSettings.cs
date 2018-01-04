using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyRoslynScript.NuGet;

namespace TestApplication
{
    public class NuGetProcessorSettings : INuGetScriptSettings
    {
        public string PackageDir  => "D:\\Sandpit\\testbucketofdoom2";
        public IEnumerable<string> SupportedPlatforms => new[]
        {
            "net461", "net46", "net452", "net451", "net45", "net403", "net40",
            "netstandard1.0", "netstandard1.1", "netstandard1.2", "netstandard1.3",
            "netstandard1.4", "netstandard1.5", "netstandard1.6", "netstandard2.0"
        };
    }
}
