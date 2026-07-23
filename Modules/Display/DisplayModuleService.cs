using System.ComponentModel;
using RenderNorth.DisplaySwitcher.Models;
using RenderNorth.DisplaySwitcher.Native;

namespace RenderNorth.DisplaySwitcher.Modules.Display;

internal sealed class DisplayModuleService
{
    public DisplayProfile Capture()
    {
        const uint flags = NativeMethods.QDC_ONLY_ACTIVE_PATHS | NativeMethods.QDC_VIRTUAL_MODE_AWARE;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            ThrowIfError(NativeMethods.GetDisplayConfigBufferSizes(flags, out var pathCount, out var modeCount), "GetDisplayConfigBufferSizes");
            var paths = new DisplayConfigPathInfo[pathCount];
            var modes = new DisplayConfigModeInfo[modeCount];
            var result = NativeMethods.QueryDisplayConfig(flags, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (result == NativeMethods.ERROR_INSUFFICIENT_BUFFER) continue;
            ThrowIfError(result, "QueryDisplayConfig");
            Array.Resize(ref paths, checked((int)pathCount));
            Array.Resize(ref modes, checked((int)modeCount));
            return new DisplayProfile
            {
                SavedAt = DateTimeOffset.Now,
                Paths = paths.ToList(),
                Modes = modes.ToList(),
                Targets = paths.Select(GetTargetIdentity).DistinctBy(target => target.MonitorDevicePath, StringComparer.OrdinalIgnoreCase).ToList()
            };
        }
        throw new InvalidOperationException("The display configuration changed repeatedly while it was being read. Try again.");
    }

    public void Validate(DisplayProfile configuration)
    {
        if (configuration.FormatVersion != 1) throw new InvalidDataException($"Unsupported display configuration version {configuration.FormatVersion}.");
        if (!string.Equals(configuration.MachineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"This display configuration was saved on {configuration.MachineName}, not {Environment.MachineName}.");
        var connected = Capture().Targets.Select(target => target.MonitorDevicePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = configuration.Targets.Where(target => !connected.Contains(target.MonitorDevicePath)).Select(target => target.FriendlyName).ToList();
        if (missing.Count > 0) throw new InvalidOperationException("Required display(s) are not connected: " + string.Join(", ", missing));
    }

    public void Apply(DisplayProfile configuration)
    {
        var flags = NativeMethods.SDC_APPLY | NativeMethods.SDC_USE_SUPPLIED_DISPLAY_CONFIG |
                    NativeMethods.SDC_SAVE_TO_DATABASE | NativeMethods.SDC_ALLOW_CHANGES | NativeMethods.SDC_VIRTUAL_MODE_AWARE;
        ThrowIfError(NativeMethods.SetDisplayConfig((uint)configuration.Paths.Count, configuration.Paths.ToArray(),
            (uint)configuration.Modes.Count, configuration.Modes.ToArray(), flags), "SetDisplayConfig");
    }

    public bool Matches(DisplayProfile expected, DisplayProfile actual)
    {
        static IEnumerable<string> Keys(DisplayProfile profile) => profile.Paths.Select(path =>
        {
            var sourceMode = profile.Modes.FirstOrDefault(mode =>
                mode.InfoType == DisplayConfigModeInfoType.Source &&
                mode.AdapterId.Value == path.SourceInfo.AdapterId.Value &&
                mode.Id == path.SourceInfo.Id);
            var source = sourceMode.InfoType == DisplayConfigModeInfoType.Source
                ? sourceMode.Mode.SourceMode
                : default;
            return $"{path.SourceInfo.AdapterId.Value}:{path.SourceInfo.Id}" +
                   $"[{source.Width}x{source.Height}@{source.Position.X},{source.Position.Y}:{source.PixelFormat}]" +
                   $">{path.TargetInfo.AdapterId.Value}:{path.TargetInfo.Id}" +
                   $"[r{(uint)path.TargetInfo.Rotation}:s{(uint)path.TargetInfo.Scaling}]";
        }).Order();
        return Keys(expected).SequenceEqual(Keys(actual));
    }

    public string DescribeTargets(DisplayProfile configuration)
    {
        var lines = configuration.Targets.OrderBy(target => target.AdapterId).ThenBy(target => target.TargetId)
            .Select((target, index) => $"Display path {index + 1}: {target.FriendlyName}\n{target.MonitorDevicePath}");
        return "Windows' numbered overlay will open next. This device list is also logged:\n\n" + string.Join("\n\n", lines);
    }

    private static SavedTargetIdentity GetTargetIdentity(DisplayConfigPathInfo path)
    {
        var packet = new DisplayConfigTargetDeviceName
        {
            Header = new DisplayConfigDeviceInfoHeader
            {
                Type = 2,
                Size = (uint)System.Runtime.InteropServices.Marshal.SizeOf<DisplayConfigTargetDeviceName>(),
                AdapterId = path.TargetInfo.AdapterId,
                Id = path.TargetInfo.Id
            },
            MonitorFriendlyDeviceName = "",
            MonitorDevicePath = ""
        };
        ThrowIfError(NativeMethods.DisplayConfigGetDeviceInfo(ref packet), "DisplayConfigGetDeviceInfo");
        return new SavedTargetIdentity
        {
            AdapterId = path.TargetInfo.AdapterId.Value,
            TargetId = path.TargetInfo.Id,
            FriendlyName = string.IsNullOrWhiteSpace(packet.MonitorFriendlyDeviceName) ? "Unnamed display" : packet.MonitorFriendlyDeviceName,
            MonitorDevicePath = packet.MonitorDevicePath
        };
    }

    private static void ThrowIfError(int error, string operation)
    {
        if (error != NativeMethods.ERROR_SUCCESS) throw new Win32Exception(error, $"{operation} failed with Windows error {error}");
    }
}
