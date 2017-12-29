using System.Collections.Generic;
using System.Linq;

namespace EasyRoslynScript.NuGet
{
    public class PackageRequirement
    {
        public static PackageRequirement Parse(string text)
        {
            var pack = new PackageRequirement();
            
            //URL format: nuget:?package=id&version=1.2.3&prerelease&Source=bla

            if(!text.StartsWith("nuget:?"))
                throw new EasyRoslynException("Nuget URL is malformed. Expected to start with nuget:?", string.Empty);

            var parts = text.Substring(7).Split('&').Select(p =>
            {
                var split = p.Split('=');

                return split.Length == 1 ? 
                new KeyValuePair<string, string>(split[0], "true") : 
                new KeyValuePair<string, string>(split[0], split[1]);
            });

            foreach (var p in parts)
            {

                switch (p.Key)
                {
                    case "package":
                        pack.Name = p.Value;
                        break;
                    case "source":
                        pack.Source = p.Value;
                        break;
                    case "version":
                        pack.Version = p.Value;
                        break;
                    case "prerelease":
                        pack.PreRelease = true;
                        break;
                    case "framework":
                        pack.Framework = p.Value;
                        break;
                    case "nodepresolve":
                        pack.NoDepResolve = true;
                        break;
                }
            }

            return pack;
        }

        public string Source { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool PreRelease { get; set; }
        public string Framework { get; set; }
        public bool NoDepResolve { get; set; }

    }
}
