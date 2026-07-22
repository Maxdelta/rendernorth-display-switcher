namespace RenderNorth.DisplaySwitcher.Domain.Environments;

internal sealed class EnvironmentCollection
{
    public const int CurrentSchemaVersion = 1;
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public List<EnvironmentDefinition> Environments { get; set; } = [];
    public Guid? ActiveEnvironmentId { get; set; }
    public DateTimeOffset? MigratedFromLegacyAt { get; set; }
    public EnvironmentActivationStatus ActivationStatus { get; set; } = new();
}

internal sealed class EnvironmentActivationStatus
{
    public string LastResult { get; set; } = "No environment activation has been recorded yet.";
    public DateTimeOffset? LastSuccessfulAt { get; set; }
}
