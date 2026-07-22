using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Services;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class EnvironmentRepositoryTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "RenderNorthEnvironmentTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SaveAndLoad_IsAtomicAndPreservesCollection()
    {
        var repository = Repository();
        var collection = Collection("Development");
        repository.Save(collection);
        var loaded = repository.Load();
        Assert.Equal(collection.Environments[0].Id, loaded.Environments[0].Id);
        Assert.Equal("Development", loaded.Environments[0].Name);
        Assert.False(File.Exists(repository.FilePath + ".tmp"));
    }

    [Fact]
    public void Load_RejectsUnsupportedCollectionSchema()
    {
        var repository = Repository();
        Directory.CreateDirectory(_folder);
        File.WriteAllText(repository.FilePath, "{\"schemaVersion\":99,\"environments\":[]}");
        var error = Assert.Throws<InvalidDataException>(() => repository.Load());
        Assert.Contains("schema 99", error.Message);
    }

    [Fact]
    public void Save_RejectsDuplicateIds()
    {
        var collection = Collection("Gaming");
        collection.Environments.Add(NewEnvironment(collection.Environments[0].Id, "Streaming"));
        Assert.Throws<InvalidDataException>(() => Repository().Save(collection));
    }

    [Fact]
    public void Save_RejectsCaseInsensitiveDuplicateNames()
    {
        var collection = Collection("Gaming");
        collection.Environments.Add(NewEnvironment(Guid.NewGuid(), " gaming "));
        Assert.Throws<InvalidDataException>(() => Repository().Save(collection));
    }

    [Fact]
    public void UnknownModule_IsPreservedAcrossLoadAndSave()
    {
        var moduleJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Modules", "unknown-module.json"));
        var module = JsonSerializer.Deserialize<ModuleDocument>(moduleJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var collection = Collection("Future Workspace");
        collection.Environments[0].Modules.Add(module);
        var repository = Repository();
        repository.Save(collection);
        var loaded = repository.Load();
        repository.Save(loaded);
        var preserved = Assert.Single(repository.Load().Environments[0].Modules);
        Assert.Equal("future-audio", preserved.Type);
        Assert.Equal(7, preserved.SchemaVersion);
        Assert.True(preserved.Data.GetProperty("nested").GetProperty("mustSurvive").GetBoolean());
    }

    private EnvironmentRepository Repository() => new(Path.Combine(_folder, "environments.json"));
    private static EnvironmentCollection Collection(string name) => new() { Environments = [NewEnvironment(Guid.NewGuid(), name)] };
    private static EnvironmentDefinition NewEnvironment(Guid id, string name) => new() { Id = id, Name = name, CreatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"), UpdatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z") };
    public void Dispose() { if (Directory.Exists(_folder)) Directory.Delete(_folder, true); }
}
