# Easy Roslyn Script
This library is a simple Roslyn script runner.

I created this project to have a common script runner between all of my side projects. The main thing that makes this project different is the way it loads scripts. You can create pre-processors which allow you to modify the script before it is executed.

I have also added the ability to add new methods via Extension methods, this makes it much more Dependency Injection friendly.

# Installation
To install the package you can search for it in the NuGet Package Manager or install from the package console with: 

```
Install-Package EasyRoslynScript
```

# Usage
First thing you need to do to use this library is create a bootstrap Script PreCompile Handler.
The purpose of this class is to specify what assemblies you want to expose to the script engine and also
what default namespaces to use.

Here is an example Script Boot Strapper that passes all loaded assemblies and imports the System namespace.
```
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
```

Next up we need to start creating commands for use in the script. You can create new classes implementing IScriptPreCompileHandler if you want full control but if you want to just add a function you can use an Extension Method.

For example:
```
public static class Foo
{
    public static void Echo(this IScriptContext context, string message)
    {
        Console.WriteLine(message);
        
    }

    public static int Sum(this IScriptContext context, int num1, int num2)
    {
        return num1 + num2;
    }
}
```
As you can see all you have to do is create an extension method for IScriptContext and it will appear in the script as a callable method.

The last step is to create the Script Runner which is what executes the script.


To do this do the following:

```
static void Main(string[] args)
{
    
    //Load in all IScriptPreCompileHandler handler objects you want to use. 
    //It is expected this will be done by a Dependancy Injection framework.
    var runner = new ScriptRunner(new IScriptPreCompileHandler[] {new ScriptBootStrap(), new ExtensionMethodHandler()});
    
    // Executes a string.
    runner.ExecuteString("Echo(Sum(1, 2).ToString());").Wait();

    //Executes a file.
    runner.ExecuteFile("myscript.csx").Wait();
}
```

You will also notice that the ExecuteMethods above are async tasks. This will allow you to use them with async and await keywords. If you don't want to just call Wait on the end like in the example above.

# NuGet Support
If you want to add NuGet Support to your scripts you can add the EasyRoslynScript.NuGet package and pass it into the constructor for the script runner.

First thing you need to do is create a settings object for it.

```
    public class NuGetProcessorSettings : INuGetScriptSettings
    {
        public string PackageDir  => "C:\\MyPackages"; //Set this to the root directory you want packages installed into.
        public IEnumerable<string> SupportedPlatforms => new[]
        {
            //List all framework versions your host process can support. You can see a list of them here: https://docs.microsoft.com/en-us/nuget/schema/target-frameworks
            "net461", "net46", "net452", "net451", "net45", "net403", "net40",
            "netstandard1.0", "netstandard1.1", "netstandard1.2", "netstandard1.3",
            "netstandard1.4", "netstandard1.5", "netstandard1.6", "netstandard2.0"
        };
    }
```

Once you have created your settings object you just need to pass an instance of it into the NuGetPreProcssor when you create it.

```
var settings = new NuGetProcessorSettings();
var nuGetProcessor = new NugetPreProcessor(settings);
var runner = new ScriptRunner(new IScriptPreCompileHandler[] {new ScriptBootStrap(), new ExtensionMethodHandler(), nuGetProcessor});
```

Now you can install nuget packages in your scripts using the #n pre processor.

```
#n nuget:?package=myPackage
#n nuget:?package=myPackage&version=1.0.0
#n nuget:?package=myPackage&version=1.0.0&source=http://mynugetserver.com/v3/index.json&prerelease&nodepresolve
```

With the #n processor you can pass in a nuget uri which will identify the package you want to install. The this url works is it starts with `nuget:?` and you put in valuename=value fromated strings split by a &.

Valid values are as follows:

| Name | Description |
|------|-------------|
| package | nuget package id to install. Not to be confused with the title |
| version | version of the package to install. If omitted will get latest version |
| source | Uri to the nuget server's v3 feed. If omitted will use nuget.org |
| prerelease | Used to indicate that pre-release versions of packages can be installed |
| framework | specify which framework the package should target. This will override what is in settings. |
| nodepresolve | This will install the nuget package but won't resolve package dependancies or try to reference the assembly. This is useful for packages that contain command line tools etc. |

## NuGet PreProcessor variables.
The following will be replaced in the script before execution with their following values:

| Name | Description |
| ---- | ----------- |
| $$NUGETDIR$$ | This will be replaced with the nuget directory specified in the settings object | 


# Bugs, Feature Requests
* Please feel free to raise issues on this repository.

# Contributing
To contribute please do the following:

* Raise an Issue on this repository.
* Fork this repository.
* Make your changes.
* Make Unit tests for your changes that pass.
* Make sure other unit tests pass.
* Rebase so your pull request is a single commit.
* Send a pull request and reference your issue.


