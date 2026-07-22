using System.Text.Json;
using RenderNorth.DisplaySwitcher.Models;
using RenderNorth.DisplaySwitcher.Modules.Display;

namespace RenderNorth.DisplaySwitcher.Services;

internal sealed class DisplayProfileService
{
    private readonly AppLogger _log;
    private readonly DisplayModuleService _display = new();
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true, IncludeFields = true };
    private string ProfilesFolder => AppPaths.ProfilesFolder;
    private string StatusPath => Path.Combine(ProfilesFolder, "status.json");

    public DisplayProfileService(AppLogger log) { _log = log; Directory.CreateDirectory(ProfilesFolder); }
    public bool HasProfile(ProfileKind kind) => File.Exists(ProfilePath(kind));

    public OperationResult Save(ProfileKind kind)
    {
        try
        {
            var profile = _display.Capture();
            var path = ProfilePath(kind);
            var temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(profile, _json));
            File.Move(temporaryPath, path, true);
            _log.Info($"Saved {kind} profile with {profile.Paths.Count} active paths and {profile.Modes.Count} modes.");
            return OperationResult.Ok($"{kind} Mode saved successfully.\n\n{Describe(profile)}");
        }
        catch (Exception exception) { _log.Error($"Failed to save {kind} profile", exception); return OperationResult.Fail($"Could not save {kind} Mode.\n\n{exception.Message}\n\nSee logs: {_log.LogFolder}"); }
    }

    public OperationResult Activate(ProfileKind kind)
    {
        DisplayProfile? rollback = null;
        try
        {
            var path = ProfilePath(kind);
            if (!File.Exists(path)) return OperationResult.Fail($"{kind} Mode has not been saved yet.");
            var saved = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(path), _json) ?? throw new InvalidDataException("The saved profile is empty.");
            _display.Validate(saved);
            rollback = _display.Capture();
            _log.Info($"Applying {kind} profile. Captured rollback configuration first.");
            _display.Apply(saved);
            if (!_display.Matches(saved, _display.Capture())) throw new InvalidOperationException("Windows accepted the request but the resulting source-to-monitor topology did not match the saved profile.");
            _log.Info($"Activated and verified {kind} profile successfully.");
            var success = OperationResult.Ok($"{kind} Mode activated successfully.");
            SaveSwitchStatus(success.Message, DateTimeOffset.Now);
            return success;
        }
        catch (Exception exception)
        {
            _log.Error($"Failed to activate {kind} profile", exception);
            var rollbackText = "No display change was made.";
            if (rollback is not null)
            {
                try { _display.Apply(rollback); rollbackText = "The previous display layout was restored."; _log.Info("Rollback succeeded."); }
                catch (Exception rollbackException) { rollbackText = "Rollback also failed; use Windows Display Settings to restore the layout."; _log.Error("Rollback failed", rollbackException); }
            }
            var failure = OperationResult.Fail($"Could not activate {kind} Mode.\n\n{exception.Message}\n\n{rollbackText}\n\nSee logs: {_log.LogFolder}");
            SaveSwitchStatus(failure.Message, null);
            return failure;
        }
    }

    public ApplicationStatus GetApplicationStatus()
    {
        var currentProfile = "Custom / Unknown";
        try
        {
            var active = _display.Capture();
            if (TryReadProfile(ProfileKind.Game, out var game) && _display.Matches(game!, active)) currentProfile = "Game Mode";
            else if (TryReadProfile(ProfileKind.Script, out var script) && _display.Matches(script!, active)) currentProfile = "Script Mode";
        }
        catch (Exception exception) { _log.Error("Could not detect the current saved profile", exception); }
        try
        {
            if (File.Exists(StatusPath))
            {
                var saved = JsonSerializer.Deserialize<PersistedSwitchStatus>(File.ReadAllText(StatusPath), _json);
                if (saved is not null) return new ApplicationStatus(currentProfile, saved.LastSwitchResult, saved.LastSuccessfulSwitchAt);
            }
        }
        catch (Exception exception) { _log.Error("Could not read the saved switch status", exception); }
        return new ApplicationStatus(currentProfile, "No switch has been recorded yet.", null);
    }

    public string IdentifyDisplays() => _display.DescribeTargets(_display.Capture());
    public void OpenWindowsIdentify() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("ms-settings:display") { UseShellExecute = true });
    private string ProfilePath(ProfileKind kind) => Path.Combine(ProfilesFolder, kind == ProfileKind.Game ? "game.json" : "script.json");
    private bool TryReadProfile(ProfileKind kind, out DisplayProfile? profile)
    {
        profile = null; var path = ProfilePath(kind); if (!File.Exists(path)) return false;
        try { profile = JsonSerializer.Deserialize<DisplayProfile>(File.ReadAllText(path), _json); return profile is not null; }
        catch (Exception exception) { _log.Error($"Could not read the saved {kind} profile while detecting status", exception); return false; }
    }
    private void SaveSwitchStatus(string result, DateTimeOffset? successfulAt)
    {
        try
        {
            DateTimeOffset? previousSuccess = null;
            if (File.Exists(StatusPath)) previousSuccess = JsonSerializer.Deserialize<PersistedSwitchStatus>(File.ReadAllText(StatusPath), _json)?.LastSuccessfulSwitchAt;
            var status = new PersistedSwitchStatus(result, successfulAt ?? previousSuccess);
            var temporaryPath = StatusPath + ".tmp";
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(status, _json)); File.Move(temporaryPath, StatusPath, true);
        }
        catch (Exception exception) { _log.Error("Could not persist the latest switch status", exception); }
    }
    private sealed record PersistedSwitchStatus(string LastSwitchResult, DateTimeOffset? LastSuccessfulSwitchAt);
    private static string Describe(DisplayProfile profile) => $"Captured {profile.Paths.Count} active display paths across {profile.Targets.Count} display devices.";
}
