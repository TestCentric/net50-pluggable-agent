#tool NuGet.CommandLine&version=6.0.0

#load nuget:?package=TestCentric.Cake.Recipe&version=1.0.0
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

ExpectedResult MockAssemblyResult => new ExpectedResult("Failed")
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

ExpectedResult AspNetCoreResult = new ExpectedResult("Passed")
{
    Assemblies = new [] { new ExpectedAssemblyResult("aspnetcore-test.dll", "Net50AgentLauncher") }
};

var packageTests = new PackageTest[] {
	// Tests of single assemblies targeting each runtime we support
	new PackageTest(
		1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
		"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult),
	new PackageTest(
		1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
		"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult),
	new PackageTest(
		1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
		"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult),
	new PackageTest(
		1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0",
		"tests/net5.0/mock-assembly.dll", MockAssemblyResult),
	// AspNetCore Tests
	new PackageTest(
		1, "AspNetCore31Test", "Run test using AspNetCore targeting .NET Core 3.1",
		"tests/netcoreapp3.1/aspnetcore-test.dll", AspNetCoreResult),
	new PackageTest(
		1, "AspNetCore50Test", "Run test using AspNetCore targeting .NET 5.0",
		"tests/netcoreapp3.1/aspnetcore-test.dll", AspNetCoreResult),
	// Windows Forms Test
    new PackageTest(
		1, "Net50WindowsFormsTest", "Run test using windows forms under .NET 5.0",
        "tests/net5.0-windows/windows-forms-test.dll",
        new ExpectedResult("Passed")
        {
            Assemblies = new [] { new ExpectedAssemblyResult("windows-forms-test.dll", "Net50AgentLauncher") }
        })
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
	testRunner: new GuiRunner("TestCentric.GuiRunner", "2.0.0-alpha8"),
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
		testRunner: new GuiRunner("testcentric-gui", "2.0.0-alpha8"),
		tests: packageTests);

BuildSettings.Packages.AddRange(new PackageDefinition[] { nugetPackage, chocolateyPackage });

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
