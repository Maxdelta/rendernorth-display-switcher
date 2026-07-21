using Velopack;
using Velopack.Sources;

namespace RenderNorth.DisplaySwitcher.Services;

internal enum UpdateState { NotChecked, Checking, UpToDate, Available, Downloading, ReadyToRestart, Failed, Offline, Portable }

internal sealed record UpdateStatus(UpdateState State, string Message, string? AvailableVersion = null,
    string? ReleaseNotes = null, int Progress = 0);

internal sealed class UpdateService
{
    // Replace only this constant if the final GitHub owner differs.
    internal const string GitHubRepositoryUrl = "https://github.com/RenderNorth/rendernorth-display-switcher";
    private readonly AppLogger _log;
    private UpdateManager? _manager;
    private UpdateInfo? _available;
    public UpdateStatus Status { get; private set; } = new(UpdateState.NotChecked, "Not checked");
    public event Action<UpdateStatus>? StatusChanged;

    public UpdateService(AppLogger log) => _log = log;

    public async Task CheckAsync()
    {
        Set(new(UpdateState.Checking, "Checking for updates…"));
        try
        {
            _manager ??= new UpdateManager(new GithubSource(GitHubRepositoryUrl, null, false));
            if (!_manager.IsInstalled)
            {
                Set(new(UpdateState.Portable, "Portable edition — automatic updates unavailable"));
                return;
            }
            _available = await _manager.CheckForUpdatesAsync();
            if (_available is null)
                Set(new(UpdateState.UpToDate, "Up to date"));
            else
                Set(new(UpdateState.Available, $"Update {_available.TargetFullRelease.Version} available",
                    _available.TargetFullRelease.Version.ToString(), _available.TargetFullRelease.NotesMarkdown));
        }
        catch (HttpRequestException ex)
        {
            _log.Error("Unable to check for updates (offline or GitHub unavailable)", ex);
            Set(new(UpdateState.Offline, "Offline / unable to check"));
        }
        catch (Exception ex)
        {
            _log.Error("Update check failed", ex);
            Set(new(UpdateState.Failed, "Update check failed"));
        }
    }

    public async Task DownloadAsync()
    {
        if (_manager is null || _available is null) return;
        try
        {
            Set(new(UpdateState.Downloading, "Downloading update…", _available.TargetFullRelease.Version.ToString()));
            var progress = new Action<int>(value => Set(new(UpdateState.Downloading,
                $"Downloading update… {value}%", _available.TargetFullRelease.Version.ToString(), Progress: value)));
            await _manager.DownloadUpdatesAsync(_available, progress);
            Set(new(UpdateState.ReadyToRestart, "Ready to restart", _available.TargetFullRelease.Version.ToString()));
        }
        catch (Exception ex)
        {
            _log.Error("Update download failed", ex);
            Set(new(UpdateState.Failed, "Update download failed"));
        }
    }

    public void ApplyAndRestart()
    {
        if (_manager is null || _available is null) return;
        _log.Info($"Applying update {_available.TargetFullRelease.Version} and restarting.");
        _manager.ApplyUpdatesAndRestart(_available.TargetFullRelease);
    }

    private void Set(UpdateStatus status)
    {
        Status = status;
        StatusChanged?.Invoke(status);
    }
}
