using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Services;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class EnvironmentManagerTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "RenderNorthManagerTests", Guid.NewGuid().ToString("N"));
    private readonly List<string> _events = [];

    [Fact]
    public async Task Activation_UsesActivationOrderAndSequentialLifecycle()
    {
        var manager = Manager(Doc("late", 200), Doc("early", 10));
        var result = await manager.ActivateAsync(Id);
        Assert.True(result.Success);
        AssertOrder("early:init", "late:init", "early:validate", "late:validate", "early:ready", "late:ready",
            "early:pre", "late:pre", "early:activate", "late:activate", "early:post", "late:post", "early:detect", "late:detect");
    }

    [Fact]
    public async Task ReadinessFailure_StopsBeforePreActivation()
    {
        var manager = Manager(Doc("blocked", 1, "readiness"));
        var result = await manager.ActivateAsync(Id);
        Assert.False(result.Success); Assert.DoesNotContain("blocked:pre", _events); Assert.Null(Repository().Load().ActiveEnvironmentId);
    }

    [Theory]
    [InlineData("activation")]
    [InlineData("post")]
    public async Task ActivationOrPostFailure_RollsBackInReverseOrder(string failure)
    {
        var manager = Manager(Doc("first", 1), Doc("second", 2, failure));
        var result = await manager.ActivateAsync(Id);
        Assert.False(result.Success);
        Assert.True(_events.IndexOf("second:rollback") < _events.IndexOf("first:rollback"));
        Assert.Null(Repository().Load().ActiveEnvironmentId);
        Assert.Null(Repository().Load().ActivationStatus.LastSuccessfulAt);
    }

    [Fact]
    public async Task UnsupportedModule_IsReportedAndNotActivated()
    {
        var manager = Manager(Doc("known", 1), Doc("unknown", 2, type: "future"));
        var result = await manager.ActivateAsync(Id);
        Assert.True(result.Success); Assert.Single(result.UnsupportedModules); Assert.DoesNotContain(_events, item => item.StartsWith("unknown:"));
    }

    [Fact]
    public void CrudOperations_UsePermanentIdsAndUniqueNames()
    {
        var manager = EmptyManager();
        var created = manager.Create("Development").Environment!;
        Assert.False(manager.Create("development").Success);
        var renamed = manager.Rename(created.Id, "Engineering").Environment!;
        Assert.Equal(created.Id, renamed.Id);
        var duplicate = manager.Duplicate(created.Id).Environment!;
        Assert.NotEqual(created.Id, duplicate.Id);
        Assert.True(manager.Move(duplicate.Id, -1).Success);
        Assert.True(manager.Delete(created.Id).Success);
        Assert.Single(manager.Load().Environments);
    }

    private static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private EnvironmentManager Manager(params ModuleDocument[] modules)
    {
        var repository = Repository(); repository.Save(new EnvironmentCollection { Environments = [new EnvironmentDefinition { Id = Id, Name = "Test", Modules = [.. modules] }] });
        return CreateManager(repository);
    }
    private EnvironmentManager EmptyManager() { var repository = Repository(); repository.Save(new EnvironmentCollection()); return CreateManager(repository); }
    private EnvironmentManager CreateManager(EnvironmentRepository repository)
    {
        var registry = new EnvironmentModuleRegistry(); registry.Register("test", () => new RecordingModule(_events));
        var id = 100; return new EnvironmentManager(repository, registry, clock: () => DateTimeOffset.Parse("2026-03-01T00:00:00Z"), newId: () => new Guid(++id, 0, 0, new byte[8]));
    }
    private static ModuleDocument Doc(string name, int order, string? failure = null, string type = "test") => new() { Type = type, SchemaVersion = 1, ActivationOrder = order, Data = JsonSerializer.SerializeToElement(new { name, failure, order }) };
    private EnvironmentRepository Repository() => new(Path.Combine(_folder, "environments.json"));
    private void AssertOrder(params string[] expected) => Assert.Equal(expected, _events);
    public void Dispose() { if (Directory.Exists(_folder)) Directory.Delete(_folder, true); }

    private sealed class RecordingModule(List<string> events) : EnvironmentModuleBase
    {
        private string _name = ""; private string? _failure; private int _order;
        public override string ModuleType => "test"; public override int SchemaVersion => 1; public override int ActivationOrder => _order;
        public override void Load(ModuleDocument document) { _name = document.Data.GetProperty("name").GetString()!; _order = document.Data.GetProperty("order").GetInt32(); if (document.Data.TryGetProperty("failure", out var failure) && failure.ValueKind == JsonValueKind.String) _failure = failure.GetString(); }
        public override ModuleDocument Save() => throw new NotSupportedException();
        public override Task InitializeAsync(ModuleContext context, CancellationToken cancellationToken) { events.Add($"{_name}:init"); return Task.CompletedTask; }
        public override Task<ModuleValidationResult> ValidateAsync(ModuleContext context, CancellationToken cancellationToken) { events.Add($"{_name}:validate"); return Task.FromResult(_failure == "validation" ? ModuleValidationResult.Invalid("blocked") : ModuleValidationResult.Valid()); }
        public override Task<ModuleReadinessResult> CanActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) { events.Add($"{_name}:ready"); return Task.FromResult(_failure == "readiness" ? ModuleReadinessResult.NotReady("blocked") : ModuleReadinessResult.Ready()); }
        public override Task<ModuleActivationResult> PreActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) { events.Add($"{_name}:pre"); return Task.FromResult(_failure == "pre" ? ModuleActivationResult.Failed("blocked") : ModuleActivationResult.Succeeded("ok")); }
        public override Task<ModuleActivationResult> ActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) { events.Add($"{_name}:activate"); return Task.FromResult(_failure == "activation" ? ModuleActivationResult.Failed("blocked", true) : ModuleActivationResult.Succeeded("ok", true)); }
        public override Task<ModuleActivationResult> PostActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) { events.Add($"{_name}:post"); return Task.FromResult(_failure == "post" ? ModuleActivationResult.Failed("blocked") : ModuleActivationResult.Succeeded("ok")); }
        public override Task<ModuleDetectionResult> DetectAsync(ModuleDetectionContext context, CancellationToken cancellationToken) { events.Add($"{_name}:detect"); return Task.FromResult(_failure == "detection" ? ModuleDetectionResult.NoMatch("blocked") : ModuleDetectionResult.Match()); }
        public override Task<ModuleDeactivationResult> DeactivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) { events.Add($"{_name}:rollback"); return Task.FromResult(ModuleDeactivationResult.Succeeded()); }
    }
}
