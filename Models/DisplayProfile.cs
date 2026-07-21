using RenderNorth.DisplaySwitcher.Native;

namespace RenderNorth.DisplaySwitcher.Models;

internal sealed class DisplayProfile
{
    public int FormatVersion { get; set; } = 1;
    public DateTimeOffset SavedAt { get; set; }
    public string MachineName { get; set; } = Environment.MachineName;
    public List<DisplayConfigPathInfo> Paths { get; set; } = [];
    public List<DisplayConfigModeInfo> Modes { get; set; } = [];
    public List<SavedTargetIdentity> Targets { get; set; } = [];
}

internal sealed class SavedTargetIdentity
{
    public long AdapterId { get; set; }
    public uint TargetId { get; set; }
    public string MonitorDevicePath { get; set; } = "";
    public string FriendlyName { get; set; } = "";
}
