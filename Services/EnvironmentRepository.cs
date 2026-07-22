using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Environments;

namespace RenderNorth.DisplaySwitcher.Services;

internal sealed class EnvironmentRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true, WriteIndented = true };

    public EnvironmentRepository(string? path = null) => _path = path ?? Path.Combine(AppPaths.DataFolder, "environments.json");
    public string FilePath => _path;

    public EnvironmentCollection Load()
    {
        if (!File.Exists(_path)) return new EnvironmentCollection();
        var collection = JsonSerializer.Deserialize<EnvironmentCollection>(File.ReadAllText(_path), _json) ?? throw new InvalidDataException("The environment collection is empty.");
        Validate(collection);
        return collection;
    }

    public void Save(EnvironmentCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        Validate(collection);
        var folder = Path.GetDirectoryName(_path) ?? throw new InvalidOperationException("The environment collection path has no parent directory.");
        Directory.CreateDirectory(folder);
        var temporaryPath = _path + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(collection, _json));
            var verification = JsonSerializer.Deserialize<EnvironmentCollection>(File.ReadAllText(temporaryPath), _json) ?? throw new InvalidDataException("The written environment collection could not be read back.");
            Validate(verification);
            File.Move(temporaryPath, _path, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public static void Validate(EnvironmentCollection collection)
    {
        if (collection.SchemaVersion != EnvironmentCollection.CurrentSchemaVersion) throw new InvalidDataException($"Unsupported environment collection schema {collection.SchemaVersion}.");
        if (collection.Environments.Any(environment => environment.Id == Guid.Empty)) throw new InvalidDataException("Environment IDs must be non-empty GUIDs.");
        var duplicateId = collection.Environments.GroupBy(environment => environment.Id).FirstOrDefault(group => group.Count() > 1);
        if (duplicateId is not null) throw new InvalidDataException($"Duplicate environment ID '{duplicateId.Key}'.");
        if (collection.Environments.Any(environment => string.IsNullOrWhiteSpace(environment.Name))) throw new InvalidDataException("Environment names cannot be blank.");
        var duplicateName = collection.Environments.GroupBy(environment => environment.Name.Trim(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
        if (duplicateName is not null) throw new InvalidDataException($"Duplicate environment name '{duplicateName.Key}'.");
        if (collection.ActiveEnvironmentId is { } activeId && collection.Environments.All(environment => environment.Id != activeId)) throw new InvalidDataException("The active environment ID does not exist in the collection.");
        foreach (var environment in collection.Environments)
        {
            if (environment.Modules.Any(module => string.IsNullOrWhiteSpace(module.Type))) throw new InvalidDataException($"Environment '{environment.Name}' contains a module without a type.");
            if (environment.Modules.Any(module => module.SchemaVersion <= 0)) throw new InvalidDataException($"Environment '{environment.Name}' contains an invalid module schema version.");
        }
    }
}
