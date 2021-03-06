﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using EasyNuGet;
using Semver;

namespace EasyRoslynScript.NuGet
{
    public class NugetPreProcessor : IScriptPreCompileHandler
    {
        private readonly INuGetScriptSettings _settings;
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        private readonly IPackageDownloader _packageDownloader;
        private readonly IPackageSearcher _packageSearcher;

        public NugetPreProcessor(INuGetScriptSettings settings, IPackageDownloader packageDownloader, IPackageSearcher packageSearcher)
        {
            _settings = settings;
            _packageDownloader = packageDownloader;
            _packageSearcher = packageSearcher;


        }

        private string ReplaceTokens(string text)
        {
            return text.Replace("$$NUGETDIR$$", _settings.PackageDir);
        }

        public string Process(string script, string folder)
        {
            var sb = new StringBuilder();

            foreach (var line in script.Split(Environment.NewLine.ToCharArray()))
            {
                try
                {
                if (!line.Trim().StartsWith("#n "))
                    {
                        sb.AppendLine(ReplaceTokens(line));
                        continue;
                    }

                    var req = PackageRequirement.Parse(line.Trim().Substring(3));

                    if (req.NoDepResolve)
                    {
                        if (!_packageDownloader.IsInstalled(req.Name, req.Version, _settings.PackageDir))
                        {
                            _packageDownloader.Download(req.Name, req.Version, _settings.PackageDir);
                        }

                        continue;
                    }

                    if (!string.IsNullOrEmpty(req.FilePath))
                    {
                        var spec = _packageDownloader.DownloadNuSpecFromFile(req.FilePath);

                        if (!_packageDownloader.IsInstalled(spec.Id, spec.Version, _settings.PackageDir))
                        {
                            _packageDownloader.DownloadFromFile(req.FilePath, _settings.PackageDir);
                        }

                        var libPath = $"{_settings.PackageDir}\\{spec.Id}\\{spec.Version}\\lib";
                        
                        //skip packages that don't contain any dll files.
                        if(!Directory.Exists(libPath))
                            continue;

                        var frameworkdir = Directory.GetDirectories(libPath).FirstOrDefault(d => req.Framework == null ? _settings.SupportedPlatforms.Contains(Path.GetFileName(d)) : d.Contains(req.Framework));

                        if (string.IsNullOrEmpty(frameworkdir))
                            continue;

                        foreach (var dll in Directory.GetFiles(frameworkdir, "*.dll", SearchOption.AllDirectories))
                            _assemblies.Add(Assembly.LoadFile(dll));

                        continue;
                    }

                    var pkg = GetPackageRequirements(new List<PackageRequirement>(), req.Name, req.Version, req.Source, req.PreRelease, req.Framework, req.NoDepResolve);

                    foreach (var p in pkg)
                    {
                        //Checking if package is already loaded into assembly.
                        //This should help reduce the extra junk downloaded like .netstandard libraries etc.
                        if(AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.FullName.ToLower().Contains(p.Name.ToLower())) != null)
                            continue;

                        if (!_packageDownloader.IsInstalled(p.Name, p.Version, _settings.PackageDir))
                        {
                            _packageDownloader.Download(p.Name, p.Version, _settings.PackageDir);
                        }

                        var libPath = $"{_settings.PackageDir}\\{p.Name}\\{p.Version}\\lib";

                        //This helps skip meta packages like NETStandard.Library.
                        if (!Directory.Exists(libPath))
                            continue;
                        

                        var frameworkdir = Directory.GetDirectories(libPath).FirstOrDefault(d => req.Framework == null ? _settings.SupportedPlatforms.Contains(Path.GetFileName(d)) : d.Contains(req.Framework));

                        if (string.IsNullOrEmpty(frameworkdir))
                            continue;

                        foreach (var dll in Directory.GetFiles(frameworkdir, "*.dll", SearchOption.AllDirectories))
                            _assemblies.Add(Assembly.LoadFile(dll));
                    }
                }
                catch (Exception e)
                {
                    throw new NuGetException($"Unable to resolve nuget package for {line}", e);
                }

            }


            return sb.ToString();
        }

        private List<PackageRequirement> GetPackageRequirements(List<PackageRequirement> requirements, string name, string version, string source, bool preRelease, string framework, bool nodeps = false)
        {
            if(requirements.All(r => r.Name != name))
                requirements.Add(new PackageRequirement
                {
                    Name = name,
                    PreRelease = preRelease,
                    Source = source ?? _settings.DefaultRepository,
                    Version = version                });


            var pkg = default(Package);

            if (version == null)
            {
                pkg = _packageSearcher.Search($"PackageId:{name}", preRelease)
                    .OrderByDescending(p => p.Version).FirstOrDefault();
            }
            else if (version.Contains("*"))
            {
                pkg = _packageSearcher.Search($"PackageId:{name}", preRelease)
                    .OrderByDescending(p => p.Version).FirstOrDefault(s => Regex.IsMatch(s.Version, version));
            }
            else if (version.Contains(","))
            {
                var versionParts = version.Split(',');
                var leftVer = versionParts[0].Replace("(", "").Replace("[", "").Trim();
                var rightVer = versionParts[1].Replace(")", "").Replace("]", "").Trim();
                var includeLeft = !versionParts[0].Contains("(");
                var includeRight = !versionParts[1].Contains(")");
                var limitRight = versionParts[1].Contains(")") || versionParts[1].Contains("]");
                var leftsemVer = string.IsNullOrEmpty(leftVer) ? new SemVersion(0) : SemVersion.Parse(leftVer);
                var rightsemVer = string.IsNullOrEmpty(rightVer) ? new SemVersion(0) : SemVersion.Parse(rightVer);

                pkg = _packageSearcher.Search($"PackageId:{name}", preRelease).OrderByDescending(p => p.Version)
                    .FirstOrDefault(
                        p =>
                        {
                            var currentVer = SemVersion.Parse(p.Version);
                            var rightPass = false;
                            var leftPass = false;

                            if (rightsemVer == new SemVersion(0))
                                rightPass = true;

                            if (limitRight && includeRight && currentVer <= rightsemVer)
                                rightPass = true;

                            if (limitRight && !includeRight && currentVer < rightsemVer)
                                rightPass = true;

                            if (!limitRight && currentVer >= rightsemVer)
                                rightPass = true;

                            if (includeLeft && currentVer >= leftsemVer)
                                leftPass = true;

                            if (!includeLeft && currentVer > leftsemVer)
                                leftPass = true;

                            if (leftsemVer == new SemVersion(0))
                                leftPass = true;

                            return leftPass && rightPass;
                        });

            }
            else
            {
                pkg = _packageSearcher.Search($"PackageId:{name}", preRelease)
                    .FirstOrDefault(s => String.Equals(s.Version, version, StringComparison.CurrentCultureIgnoreCase));
            }

            try
            {
                requirements.FirstOrDefault(r => r.Name == pkg.Id).Version = pkg.Version;
            }
            catch (Exception e)
            {
                throw new NuGetException($"Can't resolve package {name} {version}", e);
            }

            var spec = default(NuSpec);
            try
            {
                spec = _packageDownloader.DownloadNuSpec(pkg.Id, pkg.Version);
            }
            catch
            {
                _packageDownloader.DownloadArchive(pkg, $"{Environment.GetEnvironmentVariable("temp")}\\{pkg.Id}-{pkg.Version}.nupkg");
                spec = _packageDownloader.DownloadNuSpecFromFile(
                    $"{Environment.GetEnvironmentVariable("temp")}\\{pkg.Id}-{pkg.Version}.nupkg");
            }

            if (nodeps)
                return requirements;

            foreach (var dep in spec.Dependancies)
            {
                if(_settings.BlockedPackages.Any(p => string.Equals(p, dep.Id, StringComparison.CurrentCultureIgnoreCase)))
                    continue;

                if (requirements.All(r => r.Name != dep.Id))
                {
                    requirements.Add(new PackageRequirement
                    {
                        Name = dep.Id,
                        PreRelease = preRelease,
                        Source = source,
                        Version = dep.Version
                    });

                    requirements = GetPackageRequirements(requirements, dep.Id, dep.Version, source,
                        preRelease, framework);
                }
            }

            foreach (var group in spec.Groups)
            {

                var groupMatch = framework == null && _settings.SupportedPlatforms.Contains(group.Name) || framework != null && group.Name.Contains(framework) || string.IsNullOrEmpty(group.Name);
                
                if (!groupMatch) continue;

                foreach (var dep in group.Dependancies)
                {
                    if (_settings.BlockedPackages.Any(p => string.Equals(p, dep.Id, StringComparison.CurrentCultureIgnoreCase)))
                        continue;

                    if (requirements.All(r => r.Name != dep.Id))
                    {
                        requirements.Add(new PackageRequirement
                        {
                            Name = dep.Id,
                            PreRelease = preRelease,
                            Source = source,
                            Version = dep.Version
                        });

                        requirements = GetPackageRequirements(requirements, dep.Id, dep.Version, source,
                            preRelease, framework);
                    }
                }
            }

            return requirements;
        }

        public IEnumerable<Assembly> References => _assemblies;
        public IEnumerable<string> Imports => new string[]{};
        public int Priority => 100;
    }
}
