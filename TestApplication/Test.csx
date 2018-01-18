#n nuget:?file=D:\\Sandpit\\testbucketofdoom\\semver\\2.0.4\\semver.2.0.4.nupkg
using Semver;
Echo(Sum(1, 2).ToString());
var v = SemVersion.Parse("1.1.0-rc.1+nightly.2345");
Echo(v.ToString());
Echo("Hey {0} {1}", "1", 2);