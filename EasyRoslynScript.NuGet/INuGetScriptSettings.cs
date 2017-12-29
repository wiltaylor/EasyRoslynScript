using System.Collections.Generic;

namespace EasyRoslynScript.NuGet
{
    public interface INuGetScriptSettings
    {
        string PackageDir { get; }
        IEnumerable<string> SupportedPlatforms { get; }
    }
}
