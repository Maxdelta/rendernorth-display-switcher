using System.Text.Json;

namespace RenderNorth.DisplaySwitcher.Domain.Modules;

internal sealed class ModuleDocument
{
    public string Type { get; set; } = "";
    public int SchemaVersion { get; set; }
    public int ActivationOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
    public JsonElement Data { get; set; }

    public ModuleDocument Clone() => new()
    {
        Type = Type,
        SchemaVersion = SchemaVersion,
        ActivationOrder = ActivationOrder,
        IsEnabled = IsEnabled,
        Data = Data.Clone()
    };
}
