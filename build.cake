#tool NuGet.CommandLine&version=6.0.0

#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.1-dev00019
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

BuildSettings.Initialize
(
	context: Context,
	title: "Net50PluggableAgent",
	solutionFile: "net50-pluggable-agent.sln",
	unitTests: "net50-agent-launcher.tests.exe",
	githubOwner: "TestCentric",
	githubRepository: "net50-pluggable-agent"
);

BuildSettings.Packages.AddRange(new PluggableAgentFactory(".NetCoreApp, Version=5.0").Packages);

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("DumpSettings")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package")
	.IsDependentOn("Publish")
	.IsDependentOn("CreateDraftRelease")
	.IsDependentOn("CreateProductionRelease");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
