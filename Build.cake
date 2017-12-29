#tool "nuget:?package=GitVersion.CommandLine"

//Folder Variables
var RepoRootFolder = MakeAbsolute(Directory(".")); 
var InstallSourceFolder = RepoRootFolder + "/Installer";
var BuildFolder = RepoRootFolder + "/Build";
var ReleaseFolder = BuildFolder + "/Release";
var SolutionFile = RepoRootFolder + "/EasyRoslynScript.sln";
var ToolsFolder = RepoRootFolder + "/Tools";

var nugetAPIKey = EnvironmentVariable("NUGETAPIKEY");

var target = Argument("target", "Default");

GitVersion version;

try{
    version = GitVersion(new GitVersionSettings{UpdateAssemblyInfo = true}); //This updates all AssemblyInfo files automatically.
}
catch
{
    //Unable to get version.
}

Task("Default")
    .IsDependentOn("Restore")
    .IsDependentOn("Build");

Task("Restore")
    .IsDependentOn("EasyRoslynScript.Restore")
    .IsDependentOn("EasyRoslynScript_NuGet.Restore");

Task("Clean");

Task("Build")
    .IsDependentOn("EasyRoslynScript.Build")
    .IsDependentOn("EasyRoslynScript_NuGet.Build");

Task("Test");

Task("Deploy")
    .IsDependentOn("EasyRoslynScript.Deploy")
    .IsDependentOn("EasyRoslynScript_NuGet.Deploy");

Task("Version")
    .Does(() => {
        Information("Assembly Version: " + version.AssemblySemVer);
        Information("SemVer: " + version.SemVer);
        Information("Branch: " + version.BranchName);
        Information("Commit Date: " + version.CommitDate);
        Information("Build Metadata: " + version.BuildMetaData);
        Information("PreReleaseLabel: " + version.PreReleaseLabel);
        Information("FullBuildMetaData: " + version.FullBuildMetaData);
    });


/*****************************************************************************************************
EasyRoslynScript
*****************************************************************************************************/
Task("EasyRoslynScript.Clean")
    .IsDependentOn("EasyRoslynScript.Clean.Main");

Task("EasyRoslynScript.Restore")
    .IsDependentOn("EasyRoslynScript.DotNetRestore");    

Task("EasyRoslynScript.Build")
    .IsDependentOn("EasyRoslynScript.Build.Compile");

Task("EasyRoslynScript.Test");

Task("EasyRoslynScript.Deploy")
    .IsDependentOn("EasyRoslynScript.Deploy.NuGet");

Task("EasyRoslynScript.DotNetRestore")
    .Does(() => {
        var proc = StartProcess("dotnet", new ProcessSettings { Arguments = "restore", WorkingDirectory = RepoRootFolder + "/EasyRoslynScript"  });

        if(proc != 0)
            throw new Exception("dotnet didn't return 0 it returned " + proc);
    });

Task("EasyRoslynScript.UpdateVersion")
    .Does(() => {
        var file = RepoRootFolder + "/EasyRoslynScript/EasyRoslynScript.csproj";
        XmlPoke(file, "/Project/PropertyGroup/Version", version.SemVer);
        XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", version.AssemblySemVer);
        XmlPoke(file, "/Project/PropertyGroup/FileVersion", version.AssemblySemVer);
    });

Task("EasyRoslynScript.Clean.Main")
    .Does(() => 
    {
        CleanDirectory(RepoRootFolder + "/EasyRoslynScript/Bin");
    });

Task("EasyRoslynScript.Build.Compile")
    .IsDependentOn("EasyRoslynScript.UpdateVersion")
    .IsDependentOn("EasyRoslynScript.Clean.Main")
    .Does(() => {
        MSBuild(SolutionFile, config =>
            config.SetVerbosity(Verbosity.Minimal)
            .SetConfiguration("Release")
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            .SetPlatformTarget(PlatformTarget.MSIL));
        });

Task("EasyRoslynScript.Deploy.NuGet")
    .Does(() => {
        NuGetPush(RepoRootFolder + "/EasyRoslynScript/Bin/Release/EasyRoslynScript." + version.SemVer + ".nupkg",
        new NuGetPushSettings{
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = nugetAPIKey
        });
    });
    
/*****************************************************************************************************
EasyRoslynScript.NuGet
*****************************************************************************************************/
Task("EasyRoslynScript_NuGet.Clean")
    .IsDependentOn("EasyRoslynScript_NuGet.Clean.Main");

Task("EasyRoslynScript_NuGet.Restore")
    .IsDependentOn("EasyRoslynScript_NuGet.DotNetRestore");    

Task("EasyRoslynScript_NuGet.Build")
    .IsDependentOn("EasyRoslynScript_NuGet.Build.Compile");

Task("EasyRoslynScript_NuGet.Test");

Task("EasyRoslynScript_NuGet.Deploy")
    .IsDependentOn("EasyRoslynScript_NuGet.Deploy.NuGet");

Task("EasyRoslynScript_NuGet.DotNetRestore")
    .Does(() => {
        var proc = StartProcess("dotnet", new ProcessSettings { Arguments = "restore", WorkingDirectory = RepoRootFolder + "/EasyRoslynScript.NuGet"  });

        if(proc != 0)
            throw new Exception("dotnet didn't return 0 it returned " + proc);
    });

Task("EasyRoslynScript_NuGet.UpdateVersion")
    .Does(() => {
        var file = RepoRootFolder + "/EasyRoslynScript.NuGet/EasyRoslynScript.NuGet.csproj";
        XmlPoke(file, "/Project/PropertyGroup/Version", version.SemVer);
        XmlPoke(file, "/Project/PropertyGroup/AssemblyVersion", version.AssemblySemVer);
        XmlPoke(file, "/Project/PropertyGroup/FileVersion", version.AssemblySemVer);
    });

Task("EasyRoslynScript_NuGet.Clean.Main")
    .Does(() => 
    {
        CleanDirectory(RepoRootFolder + "/EasyRoslynScript.NuGet/Bin");
    });

Task("EasyRoslynScript_NuGet.Build.Compile")
    .IsDependentOn("EasyRoslynScript_NuGet.UpdateVersion")
    .IsDependentOn("EasyRoslynScript_NuGet.Clean.Main")
    .Does(() => {
        MSBuild(SolutionFile, config =>
            config.SetVerbosity(Verbosity.Minimal)
            .SetConfiguration("Release")
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .SetMSBuildPlatform(MSBuildPlatform.Automatic)
            .SetPlatformTarget(PlatformTarget.MSIL));
        });

Task("EasyRoslynScript_NuGet.Deploy.NuGet")
    .Does(() => {
        NuGetPush(RepoRootFolder + "/EasyRoslynScript.NuGet/Bin/Release/EasyRoslynScript.NuGet." + version.SemVer + ".nupkg",
        new NuGetPushSettings{
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = nugetAPIKey
        });
    });

/*****************************************************************************************************
End of script
*****************************************************************************************************/
RunTarget(target);