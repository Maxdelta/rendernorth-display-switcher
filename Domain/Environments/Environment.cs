using RenderNorth.DisplaySwitcher.Domain.Modules;

namespace RenderNorth.DisplaySwitcher.Domain.Environments;

internal sealed class EnvironmentDefinition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Icon { get; set; } = "workspace";
    public string? Category { get; set; }
    public string Accent { get; set; } = "default";
    public List<string> Tags { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsFavorite { get; set; }
    public int SortOrder { get; set; }
    public List<string> LegacyAliases { get; set; } = [];
    public List<ModuleDocument> Modules { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
