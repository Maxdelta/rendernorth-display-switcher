using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class EnvironmentModuleRegistryTests
{
    [Fact]
    public void UnknownModule_IsReportedAsUnsupportedWithoutMutation()
    {
        var document = new ModuleDocument { Type = "future-module", SchemaVersion = 1, Data = JsonSerializer.SerializeToElement(new { value = 42 }) };
        var registry = new EnvironmentModuleRegistry();
        Assert.False(registry.TryCreate(document, out var module));
        Assert.Null(module);
        Assert.Equal(42, document.Data.GetProperty("value").GetInt32());
    }

    [Fact]
    public void DuplicateRegistration_IsRejected()
    {
        var registry = new EnvironmentModuleRegistry();
        registry.Register("test", () => new TestModule());
        Assert.Throws<InvalidOperationException>(() => registry.Register("TEST", () => new TestModule()));
    }

    private sealed class TestModule : EnvironmentModuleBase
    {
        public override string ModuleType => "test";
        public override int SchemaVersion => 1;
        public override int ActivationOrder => 0;
        public override void Load(ModuleDocument document) { }
        public override ModuleDocument Save() => new() { Type = ModuleType, SchemaVersion = SchemaVersion };
        public override Task<ModuleActivationResult> ActivateAsync(ModuleActivationContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleActivationResult.Succeeded("Activated"));
        public override Task<ModuleDetectionResult> DetectAsync(ModuleDetectionContext context, CancellationToken cancellationToken) => Task.FromResult(ModuleDetectionResult.Match());
    }
}
