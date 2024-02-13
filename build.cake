#load nuget:?package=TestCentric.Cake.Recipe&version=1.1.0-dev00082
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

var target = Argument("target", Argument("t", "Default"));

BuildSettings.Initialize
(
	context: Context,
	title: "Net50PluggableAgent",
	solutionFile: "net50-pluggable-agent.sln",
	unitTests: "**/*.tests.exe",
	githubOwner: "TestCentric",
	githubRepository: "net50-pluggable-agent"
);

var MockAssemblyResult = new ExpectedResult("Failed")
{
	Total = 36, Passed = 23, Failed = 5, Warnings = 1, Inconclusive = 1, Skipped = 7,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("mock-assembly.dll") }
};


var AspNetCoreResult = new ExpectedResult("Passed")
{
	Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
	Assemblies = new ExpectedAssemblyResult[] { new ExpectedAssemblyResult("aspnetcore-test.dll") }
};

var WindowsFormsResult = new ExpectedResult("Passed")
{
	Total = 2, Passed = 2, Failed = 0, Warnings = 0, Inconclusive = 0, Skipped = 0,
	Assemblies = new ExpectedAssemblyResult[] {	new ExpectedAssemblyResult("windows-forms-test.dll") }
};

var	PackageTests = new List<PackageTest>();
PackageTests.Add(new PackageTest(
	1, "NetCore11PackageTest", "Run mock-assembly.dll targeting .NET Core 1.1",
	"tests/netcoreapp1.1/mock-assembly.dll", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, "NetCore21PackageTest", "Run mock-assembly.dll targeting .NET Core 2.1",
	"tests/netcoreapp2.1/mock-assembly.dll", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, "NetCore31PackageTest", "Run mock-assembly.dll targeting .NET Core 3.1",
	"tests/netcoreapp3.1/mock-assembly.dll", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, "Net50PackageTest", "Run mock-assembly.dll targeting .NET 5.0",
	"tests/net5.0/mock-assembly.dll --trace:Debug", MockAssemblyResult));

PackageTests.Add(new PackageTest(
	1, $"AspNetCore31Test", $"Run test using AspNetCore targeting .NET Core 3.1",
	$"tests/netcoreapp3.1/aspnetcore-test.dll", AspNetCoreResult));

PackageTests.Add(new PackageTest(
	1, $"AspNetCore50Test", $"Run test using AspNetCore targeting .NET 5.0",
	$"tests/net5.0/aspnetcore-test.dll", AspNetCoreResult));

if (!BuildSettings.IsRunningOnAppVeyor)
	PackageTests.Add(new PackageTest(
		1, "Net50WindowsFormsTest", $"Run test using windows forms under .NET 5.0",
		"tests/net5.0-windows/windows-forms-test.dll", WindowsFormsResult));

BuildSettings.Packages.Add(new NuGetPackage(
	"TestCentric.Extension.Net50PluggableAgent",
	title: ".NET 5.0 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 5.0",
	tags: new [] { "testcentric", "pluggable", "agent", "net50" },
	packageContent: new PackageContent()
		.WithRootFiles("../../LICENSE.txt", "../../README.md", "../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"net50-agent-launcher.dll", "net50-agent-launcher.pdb",
				"testcentric.extensibility.api.dll", "testcentric.engine.api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(
				"agent/net50-agent.dll", "agent/net50-agent.pdb", "agent/net50-agent.dll.config",
				"agent/net50-agent.deps.json", $"agent/net50-agent.runtimeconfig.json",
				"agent/TestCentric.Agent.Core.dll",
				"agent/testcentric.engine.api.dll", "agent/testcentric.extensibility.api.dll",
				"agent/testcentric.extensibility.dll", "agent/testcentric.metadata.dll",
				"agent/TestCentric.InternalTrace.dll",
				"agent/Microsoft.Bcl.AsyncInterfaces.dll", "agent/Microsoft.Extensions.DependencyModel.dll",
				"agent/System.Text.Encodings.Web.dll", "agent/System.Runtime.CompilerServices.Unsafe.dll",
				"agent/System.Text.Json.dll") ),
	testRunner: new AgentRunner(BuildSettings.NuGetTestDirectory + "TestCentric.Extension.Net50PluggableAgent." + BuildSettings.PackageVersion + "/tools/agent/net50-agent.dll"),
	tests: PackageTests) );
	
BuildSettings.Packages.Add(new ChocolateyPackage(
	"testcentric-extension-net50-pluggable-agent",
	title: ".NET 50 Pluggable Agent",
	description: "TestCentric engine extension for running tests under .NET 5.0",
	tags: new [] { "testcentric", "pluggable", "agent", "net50" },
	packageContent: new PackageContent()
		.WithRootFiles("../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"../../LICENSE.txt", "../../README.md", "../../VERIFICATION.txt",
				"net50-agent-launcher.dll", "net50-agent-launcher.pdb",
				"testcentric.extensibility.api.dll", "testcentric.engine.api.dll" ),
			new DirectoryContent("tools/agent").WithFiles(
				"agent/net50-agent.dll", "agent/net50-agent.pdb", "agent/net50-agent.dll.config",
				"agent/net50-agent.deps.json", $"agent/net50-agent.runtimeconfig.json",
				"agent/TestCentric.Agent.Core.dll",
				"agent/testcentric.engine.api.dll", "agent/testcentric.extensibility.api.dll",
				"agent/testcentric.extensibility.dll", "agent/testcentric.metadata.dll",
				"agent/TestCentric.InternalTrace.dll",
				"agent/Microsoft.Bcl.AsyncInterfaces.dll", "agent/Microsoft.Extensions.DependencyModel.dll",
				"agent/System.Text.Encodings.Web.dll", "agent/System.Runtime.CompilerServices.Unsafe.dll",
				"agent/System.Text.Json.dll") ),
	testRunner: new AgentRunner(BuildSettings.ChocolateyTestDirectory + "testcentric-extension-net50-pluggable-agent." + BuildSettings.PackageVersion + "/tools/agent/net50-agent.dll"),
	tests: PackageTests) );

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

RunTarget(CommandLineOptions.Target.Value);
