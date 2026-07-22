using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Services;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class CommandLineServiceTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "RenderNorthCommandTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task IdActivation_ActivatesMatchingEnvironment()
    {
        var setup = Setup(); var result = await setup.Commands.ExecuteAsync(["--environment-id", setup.Id.ToString()]);
        Assert.Equal(0, result.ExitCode); Assert.Equal(setup.Id, setup.Repository.Load().ActiveEnvironmentId);
    }

    [Fact]
    public async Task NameActivation_IsCaseInsensitive()
    {
        var setup = Setup(); var result = await setup.Commands.ExecuteAsync(["--environment", "gAmInG"]);
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task MissingEnvironment_ReturnsDocumentedExitCode()
    {
        var setup = Setup(); var result = await setup.Commands.ExecuteAsync(["--environment", "Missing"]);
        Assert.Equal(CommandLineService.MissingEnvironmentExitCode, result.ExitCode);
    }

    [Fact]
    public void AmbiguousName_IsRejectedEvenIfInvalidDataReachesResolutionBoundary()
    {
        var environments = new[]
        {
            new EnvironmentDefinition { Id = Guid.NewGuid(), Name = "Gaming" },
            new EnvironmentDefinition { Id = Guid.NewGuid(), Name = "gaming" }
        };
        var result = CommandLineService.ResolveByName(environments, "GAMING");
        Assert.Equal(CommandLineService.AmbiguousEnvironmentExitCode, result.ExitCode);
        Assert.Null(result.EnvironmentId);
    }

    [Theory]
    [InlineData("--game")]
    [InlineData("--script")]
    public async Task LegacyAlias_ActivatesMigratedEnvironment(string argument)
    {
        var setup = Setup(argument.TrimStart('-')); var result = await setup.Commands.ExecuteAsync([argument]);
        Assert.Equal(0, result.ExitCode);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("--environment-id", "not-a-guid")]
    [InlineData("--silent")]
    public async Task InvalidArguments_ReturnExitCodeTwo(params string[] arguments)
    {
        var result = await Setup().Commands.ExecuteAsync(arguments);
        Assert.Equal(CommandLineService.InvalidArgumentsExitCode, result.ExitCode);
    }

    [Fact]
    public async Task ListEnvironments_WritesStableIdAndName()
    {
        var output = new StringWriter(); var setup = Setup(output: output);
        var result = await setup.Commands.ExecuteAsync(["--list-environments"]);
        Assert.Equal(0, result.ExitCode); Assert.Contains(setup.Id.ToString(), output.ToString()); Assert.Contains("Gaming", output.ToString());
    }

    [Fact]
    public async Task ActivationCommand_DoesNotRequestGui()
    {
        var setup = Setup(); var result = await setup.Commands.ExecuteAsync(["--environment-id", setup.Id.ToString(), "--silent"]);
        Assert.False(result.ShowGui); Assert.Equal(0, result.ExitCode);
    }

    private SetupResult Setup(string alias = "game", TextWriter? output = null)
    {
        var id = Guid.NewGuid(); var repository = new EnvironmentRepository(Path.Combine(_folder, Guid.NewGuid() + ".json"));
        repository.Save(new EnvironmentCollection { Environments = [new EnvironmentDefinition { Id = id, Name = "Gaming", LegacyAliases = [alias], Modules = [Document()] }] });
        var registry = new EnvironmentModuleRegistry(); registry.Register("test", () => new SuccessfulModule());
        var manager = new EnvironmentManager(repository, registry);
        return new SetupResult(id, repository, new CommandLineService(manager, output: output));
    }
    private static ModuleDocument Document() => new() { Type = "test", SchemaVersion = 1, Data = JsonSerializer.SerializeToElement(new { }) };
    public void Dispose() { if (Directory.Exists(_folder)) Directory.Delete(_folder, true); }
    private sealed record SetupResult(Guid Id, EnvironmentRepository Repository, CommandLineService Commands);
    private sealed class SuccessfulModule : EnvironmentModuleBase
    {
        public override string ModuleType => "test"; public override int SchemaVersion => 1; public override int ActivationOrder => 1;
        public override void Load(ModuleDocument document) { } public override ModuleDocument Save() => Document();
        public override Task<ModuleActivationResult> ActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleActivationResult.Succeeded("ok", true));
        public override Task<ModuleDetectionResult> DetectAsync(ModuleDetectionContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleDetectionResult.Match());
    }
}
