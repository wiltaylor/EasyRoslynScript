﻿using System.Collections.Generic;
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

        public string DefaultRepository => "https://www.myget.org/F/fcepacks/api/v3/index.json";
        public IEnumerable<string> BlockedPackages => new[] { "NETStandard.Library", "Microsoft.Win32.Primitives", "Microsoft.NETCore.Targets", "Microsoft.NETCore.Platforms" };
    }
}
