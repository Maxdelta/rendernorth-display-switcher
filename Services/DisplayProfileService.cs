using System.ComponentModel;
using System.Text.Json;
using RenderNorth.DisplaySwitcher.Models;
using RenderNorth.DisplaySwitcher.Native;

namespace RenderNorth.DisplaySwitcher.Services;

internal sealed class DisplayProfileService
{
    private readonly AppLogger _log;
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true, IncludeFields = true };
    private string ProfilesFolder => AppPaths.ProfilesFolder;
    private string StatusPath => Path.Combine(ProfilesFolder, "status.json");

    public DisplayProfileService(AppLogger log)
    {
        _log = log;
        Directory.CreateDirectory(ProfilesFolder);
    }

    public bool HasProfile(ProfileKind kind) => File.Exists(ProfilePath(kind));

    public OperationResult Save(ProfileKind kind)
    {
        try
        {
            var profile = Capture();
            var path = ProfilePath(kind);
            var temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profile, _json));
            File.Move(temporaryPath, path, true);
            _log.Info($"Saved {kind} profile with {profile.Paths.Count} active paths and {profile.Modes.Count} modes.");
            return OperationResult.Ok($"{kind} Mode saved successfully.\n\n{Describe(profile)}");
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to save {kind} profile", ex);
            return OperationResult.Fail($"Could not save {kind} Mode.\n\n{ex.Message}\n\nSee logs: {_log.LogFolder}");
        }
    }

    public OperationResult Activate(ProfileKind kind)
    {
        DisplayProfile? rollback = null;
        try
        {
            var path = ProfilePath(kind);
            if (!File.Exists(path)) return OperationResult.Fail($"{kind} Mode has not been saved yet.");
            var saved = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(path), _json)
                ?? throw new InvalidDataException("The saved profile is empty.");
            ValidateMachine(saved);
            ValidateConnectedTargets(saved);

            rollback = Capture();
            _log.Info($"Applying {kind} profile. Captured rollback configuration first.");
            Apply(saved);

            var active = Capture();
            if (!SameTopology(saved, active))
                throw new InvalidOperationException("Windows accepted the request but the resulting source-to-monitor topology did not match the saved profile.");

            _log.Info($"Activated and verified {kind} profile successfully.");
            var success = OperationResult.Ok($"{kind} Mode activated successfully.");
            SaveSwitchStatus(success.Message, DateTimeOffset.Now);
            return success;
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to activate {kind} profile", ex);
            var rollbackText = "No display change was made.";
            if (rollback is not null)
            {
                try { Apply(rollback); rollbackText = "The previous display layout was restored."; _log.Info("Rollback succeeded."); }
                catch (Exception rollbackEx) { rollbackText = "Rollback also failed; use Windows Display Settings to restore the layout."; _log.Error("Rollback failed", rollbackEx); }
            }
            var failure = OperationResult.Fail($"Could not activate {kind} Mode.\n\n{ex.Message}\n\n{rollbackText}\n\nSee logs: {_log.LogFolder}");
            SaveSwitchStatus(failure.Message, null);
            return failure;
        }
    }

    public ApplicationStatus GetApplicationStatus()
    {
        var currentProfile = "Custom / Unknown";
        try
        {
            var active = Capture();
            if (TryReadProfile(ProfileKind.Game, out var game) && SameTopology(game!, active))
                currentProfile = "Game Mode";
            else if (TryReadProfile(ProfileKind.Script, out var script) && SameTopology(script!, active))
                currentProfile = "Script Mode";
        }
        catch (Exception ex)
        {
            _log.Error("Could not detect the current saved profile", ex);
        }

        try
        {
            if (File.Exists(StatusPath))
            {
                var saved = JsonSerializer.Deserialize<PersistedSwitchStatus>(File.ReadAllText(StatusPath), _json);
                if (saved is not null)
                    return new ApplicationStatus(currentProfile, saved.LastSwitchResult, saved.LastSuccessfulSwitchAt);
            }
        }
        catch (Exception ex)
        {
            _log.Error("Could not read the saved switch status", ex);
        }

        return new ApplicationStatus(currentProfile, "No switch has been recorded yet.", null);
    }

    public string IdentifyDisplays()
    {
        var profile = Capture();
        var lines = profile.Targets.OrderBy(t => t.AdapterId).ThenBy(t => t.TargetId)
            .Select((t, i) => $"Display path {i + 1}: {t.FriendlyName}\n{t.MonitorDevicePath}");
        return "Windows' numbered overlay will open next. This device list is also logged:\n\n" + string.Join("\n\n", lines);
    }

    public void OpenWindowsIdentify() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-settings:display") { UseShellExecute = true });

    private DisplayProfile Capture()
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
                Targets = paths.Select(GetTargetIdentity).DistinctBy(x => x.MonitorDevicePath, StringComparer.OrdinalIgnoreCase).ToList()
            };
        }
        throw new InvalidOperationException("The display configuration changed repeatedly while it was being read. Try again.");
    }

    private void Apply(DisplayProfile profile)
    {
        var flags = NativeMethods.SDC_APPLY | NativeMethods.SDC_USE_SUPPLIED_DISPLAY_CONFIG |
                    NativeMethods.SDC_SAVE_TO_DATABASE | NativeMethods.SDC_ALLOW_CHANGES | NativeMethods.SDC_VIRTUAL_MODE_AWARE;
        ThrowIfError(NativeMethods.SetDisplayConfig((uint)profile.Paths.Count, profile.Paths.ToArray(),
            (uint)profile.Modes.Count, profile.Modes.ToArray(), flags), "SetDisplayConfig");
    }

    private SavedTargetIdentity GetTargetIdentity(DisplayConfigPathInfo path)
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

    private void ValidateConnectedTargets(DisplayProfile saved)
    {
        var currentPaths = Capture();
        var connected = currentPaths.Targets.Select(t => t.MonitorDevicePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = saved.Targets.Where(t => !connected.Contains(t.MonitorDevicePath)).Select(t => t.FriendlyName).ToList();
        if (missing.Count > 0) throw new InvalidOperationException("Required display(s) are not connected: " + string.Join(", ", missing));
    }

    private static bool SameTopology(DisplayProfile expected, DisplayProfile actual)
    {
        static IEnumerable<string> Keys(DisplayProfile p) => p.Paths.Select(x =>
            $"{x.SourceInfo.AdapterId.Value}:{x.SourceInfo.Id}>{x.TargetInfo.AdapterId.Value}:{x.TargetInfo.Id}").Order();
        return Keys(expected).SequenceEqual(Keys(actual));
    }

    private static void ValidateMachine(DisplayProfile profile)
    {
        if (profile.FormatVersion != 1) throw new InvalidDataException($"Unsupported profile version {profile.FormatVersion}.");
        if (!string.Equals(profile.MachineName, Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"This profile was saved on {profile.MachineName}, not {Environment.MachineName}.");
    }

    private string ProfilePath(ProfileKind kind) => Path.Combine(ProfilesFolder, kind == ProfileKind.Game ? "game.json" : "script.json");

    private bool TryReadProfile(ProfileKind kind, out DisplayProfile? profile)
    {
        profile = null;
        var path = ProfilePath(kind);
        if (!File.Exists(path)) return false;
        try
        {
            profile = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(path), _json);
            return profile is not null;
        }
        catch (Exception ex)
        {
            _log.Error($"Could not read the saved {kind} profile while detecting status", ex);
            return false;
        }
    }

    private void SaveSwitchStatus(string result, DateTimeOffset? successfulAt)
    {
        try
        {
            DateTimeOffset? previousSuccess = null;
            if (File.Exists(StatusPath))
                previousSuccess = JsonSerializer.Deserialize<PersistedSwitchStatus>(File.ReadAllText(StatusPath), _json)?.LastSuccessfulSwitchAt;
            var status = new PersistedSwitchStatus(result, successfulAt ?? previousSuccess);
            var temporaryPath = StatusPath + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(status, _json));
            File.Move(temporaryPath, StatusPath, true);
        }
        catch (Exception ex)
        {
            _log.Error("Could not persist the latest switch status", ex);
        }
    }

    private sealed record PersistedSwitchStatus(string LastSwitchResult, DateTimeOffset? LastSuccessfulSwitchAt);
    private static string Describe(DisplayProfile p) => $"Captured {p.Paths.Count} active display paths across {p.Targets.Count} display devices.";
    private static void ThrowIfError(int error, string operation)
    {
        if (error != NativeMethods.ERROR_SUCCESS) throw new Win32Exception(error, $"{operation} failed with Windows error {error}");
    }
}
