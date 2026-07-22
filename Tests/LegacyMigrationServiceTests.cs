using RenderNorth.DisplaySwitcher.Services;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class LegacyMigrationServiceTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "RenderNorthLegacyMigrationTests", Guid.NewGuid().ToString("N"));
    private static readonly DateTimeOffset MigrationTime = DateTimeOffset.Parse("2026-02-03T04:05:06Z");
    private int _id;

    [Fact]
    public void NoProfiles_DoesNotCreateCollectionOrBackup()
    {
        var result = Service().MigrateIfNeeded();
        Assert.False(result.MigrationAttempted);
        Assert.False(File.Exists(Repository().FilePath));
        Assert.Null(result.BackupFolder);
    }

    [Theory]
    [InlineData("game.json", "Game Mode", "game")]
    [InlineData("script.json", "Script Mode", "script")]
    public void SingleProfile_MigratesWithStableLegacyAlias(string fileName, string name, string alias)
    {
        CopyFixture(fileName);
        var result = Service().MigrateIfNeeded();
        var environment = Assert.Single(Repository().Load().Environments);
        Assert.True(result.CollectionCreated);
        Assert.Equal(name, environment.Name);
        Assert.Contains(alias, environment.LegacyAliases);
        Assert.Single(environment.Modules);
    }

    [Fact]
    public void BothProfiles_MigrateInApprovedOrder()
    {
        CopyFixture("game.json"); CopyFixture("script.json");
        var result = Service().MigrateIfNeeded();
        var environments = Repository().Load().Environments;
        Assert.Equal(2, result.MigratedCount);
        Assert.Collection(environments.OrderBy(item => item.SortOrder),
            game => Assert.Equal("Game Mode", game.Name),
            script => Assert.Equal("Script Mode", script.Name));
    }

    [Fact]
    public void CorruptProfile_IsBackedUpButDoesNotCreateInvalidCollection()
    {
        CopyFixture("corrupt.json", "game.json");
        var result = Service().MigrateIfNeeded();
        Assert.True(result.MigrationAttempted);
        Assert.False(result.CollectionCreated);
        Assert.Single(result.Errors);
        Assert.True(File.Exists(Path.Combine(result.BackupFolder!, "game.json")));
        Assert.False(File.Exists(Repository().FilePath));
    }

    [Fact]
    public void RepeatedStartup_DoesNotDuplicateEnvironmentsOrBackups()
    {
        CopyFixture("game.json");
        var service = Service();
        var first = service.MigrateIfNeeded();
        var second = service.MigrateIfNeeded();
        Assert.True(first.CollectionCreated);
        Assert.False(second.MigrationAttempted);
        Assert.Single(Repository().Load().Environments);
        Assert.Single(Directory.GetDirectories(Path.Combine(_folder, "migration-backups")));
    }

    [Fact]
    public void ExistingEnvironmentCollection_PreventsMigration()
    {
        Repository().Save(new RenderNorth.DisplaySwitcher.Domain.Environments.EnvironmentCollection());
        CopyFixture("game.json");
        var result = Service().MigrateIfNeeded();
        Assert.False(result.MigrationAttempted);
        Assert.Empty(Repository().Load().Environments);
    }

    [Fact]
    public void BackupIsCreatedAndLegacyFilesRemainUntouched()
    {
        CopyFixture("game.json"); CopyFixture("script.json");
        var gamePath = Path.Combine(_folder, "profiles", "game.json");
        var before = File.ReadAllBytes(gamePath);
        var result = Service().MigrateIfNeeded();
        Assert.NotNull(result.BackupFolder);
        Assert.True(File.Exists(Path.Combine(result.BackupFolder!, "game.json")));
        Assert.True(File.Exists(Path.Combine(result.BackupFolder!, "script.json")));
        Assert.Equal(before, File.ReadAllBytes(gamePath));
    }

    private LegacyMigrationService Service() => new(Repository(), _folder, clock: () => MigrationTime,
        newId: () => new Guid(++_id, 0, 0, new byte[8]));
    private EnvironmentRepository Repository() => new(Path.Combine(_folder, "environments.json"));
    private void CopyFixture(string fixture, string? destination = null)
    {
        var profiles = Path.Combine(_folder, "profiles"); Directory.CreateDirectory(profiles);
        File.Copy(Path.Combine(AppContext.BaseDirectory, "Fixtures", "LegacyProfiles", fixture), Path.Combine(profiles, destination ?? fixture), true);
    }
    public void Dispose() { if (Directory.Exists(_folder)) Directory.Delete(_folder, true); }
}
