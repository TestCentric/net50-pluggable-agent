#tool NuGet.CommandLine&version=6.0.0
#tool nuget:?package=GitVersion.CommandLine&version=5.6.3
#tool nuget:?package=GitReleaseManager&version=0.12.1

// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.0-dev00040
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

static readonly string GUI_RUNNER = "tools/testcentric.exe";

BuildSettings.Initialize
(
	context: Context,
	title: "NetCore50PluggableAgent",
	solutionFile: "net50-pluggable-agent.sln",
	unitTests: "net50-agent-launcher.tests.exe",
	guiVersion: "2.0.0-dev00226",
	githubOwner: "TestCentric",
	githubRepository: "net50-pluggable-agent"
);

Information($"Net50PluggableAgent {BuildSettings.Configuration} version {BuildSettings.PackageVersion}");

if (BuildSystem.IsRunningOnAppVeyor)
	AppVeyor.UpdateBuildVersion(BuildSettings.PackageVersion + "-" + AppVeyor.Environment.Build.Number);

var packageTests = new PackageTest[] {
//	new PackageTest(
//		1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
//		"tests/netcoreapp1.1/mock-assembly.dll --run --unattended", CommonResult),
//	new PackageTest(
//		1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
//		"tests/netcoreapp2.1/mock-assembly.dll --run --unattended", CommonResult),
//	new PackageTest(
//		1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
//		"tests/netcoreapp3.1/mock-assembly.dll --run --unattended", CommonResult),
	new PackageTest(
		1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0",
		"tests/net5.0/mock-assembly.dll --run --unattended", CommonResult)
};

var nugetPackage = new NuGetPackage(
	id: "NUnit.Extension.Net50PluggableAgent",
	source: "nuget/Net50PluggableAgent.nuspec",
	basePath: BuildSettings.OutputDirectory,
	checks: new PackageCheck[] {
		HasFiles("LICENSE.txt", "CHANGES.txt"),
		HasDirectory("tools").WithFiles("net50-agent-launcher.dll", "nunit.engine.api.dll"),
		HasDirectory("tools/agent").WithFiles(
			"net50-pluggable-agent.dll", "net50-pluggable-agent.dll.config",
			"nunit.engine.api.dll", "testcentric.engine.core.dll",
			"testcentric.engine.metadata.dll", "testcentric.extensibility.dll") },
	testRunner: new GuiRunner("TestCentric.GuiRunner", "2.0.0-dev00226"),
	tests: packageTests );

var chocolateyPackage = new ChocolateyPackage(
		id: "nunit-extension-net50-pluggable-agent",
		source: "choco/net50-pluggable-agent.nuspec",
		basePath: BuildSettings.OutputDirectory,
		checks: new PackageCheck[] {
			HasDirectory("tools").WithFiles("net50-agent-launcher.dll", "nunit.engine.api.dll")
				.WithFiles("LICENSE.txt", "CHANGES.txt", "VERIFICATION.txt"),
			HasDirectory("tools/agent").WithFiles(
				"net50-pluggable-agent.dll", "net50-pluggable-agent.dll.config",
				"nunit.engine.api.dll", "testcentric.engine.core.dll",
				"testcentric.engine.metadata.dll", "testcentric.extensibility.dll") },
		testRunner: new GuiRunner("testcentric-gui", "2.0.0-dev00226"),
		tests: packageTests);

BuildSettings.Packages.AddRange(new PackageDefinition[] { nugetPackage, chocolateyPackage });

ExpectedResult CommonResult => new ExpectedResult("Failed")
{
	Total = 36,
	Passed = 23,
	Failed = 5,
	Warnings = 1,
	Inconclusive = 1,
	Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[]
	{
		new ExpectedAssemblyResult("mock-assembly.dll", "Net50AgentLauncher")
	}
};

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Appveyor")
	.IsDependentOn("BuildTestAndPackage")
	.IsDependentOn("Publish");

Task("BuildTestAndPackage")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

//Task("Travis")
//	.IsDependentOn("Build")
//	.IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
