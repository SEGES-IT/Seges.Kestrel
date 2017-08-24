#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.1
#tool nuget:?package=NUnit.Extension.TeamCityEventListener
#tool "nuget:?package=gitlink"
#addin nuget:?package=Cake.DoInDirectory

var target = Argument("target", "Default");
var configuration = "Release";
var output = Directory("build");
var solutions = GetFiles("./**/*.sln");

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Package");

Task("Clean")
    .Does(() =>
    {
        foreach(var solution in solutions)
        {
            DotNetCoreClean(solution.GetDirectory().FullPath, new DotNetCoreCleanSettings{Configuration = "Debug"});
            DotNetCoreClean(solution.GetDirectory().FullPath, new DotNetCoreCleanSettings{Configuration = "Release"});
        }
        CleanDirectory(output);
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        foreach(var solution in solutions)
        {
            DotNetCoreBuild(solution.GetDirectory().FullPath, new DotNetCoreBuildSettings {
                Configuration = configuration,
            });
        }
    });

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach(var solution in solutions)
        {
            var testProjects = GetFiles(solution.GetDirectory().FullPath + "/**/bin/**/*Tests.dll");
            foreach (var testProject in testProjects)
            {
                //DotNetCoreTest(testProject.FullPath, new DotNetCoreTestSettings {
                //    Configuration = configuration,
                //});
                NUnit3(testProject.FullPath, new NUnit3Settings {
                    Configuration = configuration,
                    Full = true,
                    TeamCity = true,
                    Where = "cat==BuildVerification"
                });
            }
        }
    });

Task("Restore")
    .Does(() =>
    {
        foreach(var solution in solutions)
        {
            DotNetCoreRestore(solution.GetDirectory().FullPath, new DotNetCoreRestoreSettings {
            });
        }
    });


Task("GitLink")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var pdbsInSolution = GetFiles(Context.Environment.WorkingDirectory + "/src/**/*.pdb");
	    Func<FilePath, bool> recentlyUpdated = (FilePath f) => (DateTime.UtcNow.Subtract(new FileInfo(f.FullPath).LastWriteTimeUtc).TotalMinutes < 5);
	    foreach (var pdb in pdbsInSolution.Where(recentlyUpdated))
        {
            GitLink(pdb.FullPath, new GitLinkSettings {});
        }
    });


Task("Package")
    .IsDependentOn("Test")
    .IsDependentOn("GitLink")
    .Does(() =>
    {
        foreach(var solution in solutions)
        {
    	    Func<FilePath, bool> notTestProject = (FilePath f) => (!f.FullPath.EndsWith("Tests.csproj"));
            var csprojsInSolution = GetFiles(solution.GetDirectory().FullPath + "/**/*.csproj").Where(notTestProject);
            foreach (var csProj in csprojsInSolution)
            {
                DotNetCorePack(csProj.GetDirectory().FullPath, new DotNetCorePackSettings {
                    Configuration = configuration,
                    OutputDirectory = output,
                    IncludeSymbols = true,
                    NoBuild = true
                });
            }
        }
});

RunTarget(target);