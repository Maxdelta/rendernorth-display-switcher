using System.Text.Json;
using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Models;
using RenderNorth.DisplaySwitcher.Modules.Display;

namespace RenderNorth.DisplaySwitcher.Services;

internal sealed record LegacyMigrationResult(
    bool MigrationAttempted,
    bool CollectionCreated,
    int MigratedCount,
    IReadOnlyList<string> Errors,
    string? BackupFolder);

internal sealed class LegacyMigrationService
{
    private static readonly JsonSerializerOptions LegacyJson = new() { IncludeFields = true, PropertyNameCaseInsensitive = true };
    private readonly EnvironmentRepository _repository;
    private readonly string _dataFolder;
    private readonly AppLogger? _log;
    private readonly Func<DateTimeOffset> _clock;
    private readonly Func<Guid> _newId;

    public LegacyMigrationService(EnvironmentRepository repository, string dataFolder, AppLogger? log = null,
        Func<DateTimeOffset>? clock = null, Func<Guid>? newId = null)
    {
        _repository = repository;
        _dataFolder = dataFolder;
        _log = log;
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _newId = newId ?? Guid.NewGuid;
    }

    public LegacyMigrationResult MigrateIfNeeded()
    {
        if (File.Exists(_repository.FilePath))
        {
            _repository.Load();
            Info("Environment collection already exists; legacy migration was not repeated.");
            return new LegacyMigrationResult(false, false, 0, [], null);
        }

        var profilesFolder = Path.Combine(_dataFolder, "profiles");
        var sources = new[]
        {
            new LegacySource("game.json", "Game Mode", "game", "gamepad", "Gaming", 0),
            new LegacySource("script.json", "Script Mode", "script", "script", "Streaming", 1)
        }.Where(source => File.Exists(Path.Combine(profilesFolder, source.FileName))).ToArray();

        if (sources.Length == 0)
        {
            Info("No legacy Game Mode or Script Mode profiles were found.");
            return new LegacyMigrationResult(false, false, 0, [], null);
        }

        var migrationTime = _clock();
        var backupFolder = CreateBackupFolder(migrationTime);
        foreach (var source in sources)
            File.Copy(Path.Combine(profilesFolder, source.FileName), Path.Combine(backupFolder, source.FileName), false);
        var statusPath = Path.Combine(profilesFolder, "status.json");
        if (File.Exists(statusPath)) File.Copy(statusPath, Path.Combine(backupFolder, "status.json"), false);
        Info($"Backed up legacy profile data to {backupFolder}.");

        var environments = new List<EnvironmentDefinition>();
        var errors = new List<string>();
        foreach (var source in sources)
        {
            var sourcePath = Path.Combine(profilesFolder, source.FileName);
            try
            {
                var configuration = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(sourcePath), LegacyJson)
                    ?? throw new InvalidDataException("The profile is empty.");
                if (configuration.FormatVersion != 1) throw new InvalidDataException($"Unsupported legacy profile version {configuration.FormatVersion}.");
                var displayModule = new DisplayModule(new DisplayModuleService());
                displayModule.SetConfiguration(configuration);
                environments.Add(new EnvironmentDefinition
                {
                    Id = _newId(),
                    Name = source.Name,
                    Icon = source.Icon,
                    Category = source.Category,
                    CreatedAt = configuration.SavedAt == default ? migrationTime : configuration.SavedAt,
                    UpdatedAt = migrationTime,
                    IsFavorite = true,
                    SortOrder = source.SortOrder,
                    LegacyAliases = [source.Alias],
                    Modules = [displayModule.Save()],
                    Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["legacySource"] = source.FileName }
                });
                Info($"Migrated legacy {source.Name} profile into an environment.");
            }
            catch (Exception exception)
            {
                var message = $"Could not migrate {source.FileName}: {exception.Message}";
                errors.Add(message);
                Error(message, exception);
            }
        }

        if (environments.Count == 0)
        {
            Error("Legacy migration produced no valid environments; the environment collection was not created.");
            return new LegacyMigrationResult(true, false, 0, errors, backupFolder);
        }

        _repository.Save(new EnvironmentCollection
        {
            Environments = environments,
            MigratedFromLegacyAt = migrationTime
        });
        Info($"Legacy migration created {environments.Count} environment(s).");
        return new LegacyMigrationResult(true, true, environments.Count, errors, backupFolder);
    }

    private string CreateBackupFolder(DateTimeOffset timestamp)
    {
        var root = Path.Combine(_dataFolder, "migration-backups");
        Directory.CreateDirectory(root);
        var baseName = $"display-switcher-v0.3.2-{timestamp:yyyyMMdd-HHmmss}";
        var candidate = Path.Combine(root, baseName);
        for (var suffix = 1; Directory.Exists(candidate); suffix++) candidate = Path.Combine(root, $"{baseName}-{suffix}");
        Directory.CreateDirectory(candidate);
        return candidate;
    }

    private void Info(string message) => _log?.Info(message);
    private void Error(string message, Exception? exception = null) => _log?.Error(message, exception);
    private sealed record LegacySource(string FileName, string Name, string Alias, string Icon, string Category, int SortOrder);
}
