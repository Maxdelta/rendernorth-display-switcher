namespace RenderNorth.DisplaySwitcher.Domain.Environments;

internal sealed class EnvironmentCollection
{
    public const int CurrentSchemaVersion = 1;
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public List<EnvironmentDefinition> Environments { get; set; } = [];
    public Guid? ActiveEnvironmentId { get; set; }
    public DateTimeOffset? MigratedFromLegacyAt { get; set; }
}
