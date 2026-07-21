using System.Diagnostics;
using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class MainForm : Form
{
    private static readonly Color Background = Color.FromArgb(24, 29, 33);
    private static readonly Color Card = Color.FromArgb(35, 42, 47);
    private static readonly Color Accent = Color.FromArgb(38, 198, 190);
    private static readonly Color Muted = Color.FromArgb(174, 187, 194);
    private readonly DisplayProfileService _service;
    private readonly UpdateService _updates;
    private readonly AppLogger _log;
    private readonly Label _activeProfile = TextLabel("Custom / Unknown", 18, true);
    private readonly Panel _indicator = new() { Size = new Size(12, 12) };
    private readonly Label _lastResult = TextLabel("No switch recorded", 9);
    private readonly Label _lastSuccessful = TextLabel("None recorded", 9);
    private readonly Label _updateStatus = TextLabel("Not checked", 9);
    private readonly Label _releaseNotes = TextLabel("", 8);
    private readonly Button _downloadButton;

    public MainForm(DisplayProfileService service, UpdateService updates, AppLogger log)
    {
        _service = service; _updates = updates; _log = log;
        Text = "RenderNorth Display Switcher";
        ClientSize = new Size(620, 690); MinimumSize = new Size(636, 729);
        StartPosition = FormStartPosition.CenterScreen; BackColor = Background; ForeColor = Color.White;
        Font = new Font("Segoe UI", 9); AutoScaleMode = AutoScaleMode.Dpi;

        var root = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(16), BackColor = Background };
        root.Controls.Add(Header());
        root.Controls.Add(ActiveCard());
        root.Controls.Add(ModeCard("GAME MODE", "Main Gaming Monitor → Elgato\nSecond monitor remains extended", "Activate Game Mode", ProfileKind.Game, Color.FromArgb(35, 174, 108)));
        root.Controls.Add(ModeCard("SCRIPT MODE", "Second Monitor → Elgato\nMain monitor remains private", "Activate Script Mode", ProfileKind.Script, Accent));
        root.Controls.Add(ProfileManagement());
        _downloadButton = ActionButton("Download and Install", Accent, async (_, _) => await DownloadAndInstallAsync());
        _downloadButton.Visible = false;
        root.Controls.Add(Utilities());
        root.Controls.Add(StatusCard());
        Controls.Add(root);

        _updates.StatusChanged += OnUpdateStatusChanged;
        Shown += async (_, _) => { RefreshProfileStatus(); await _updates.CheckAsync(); };
    }

    private Control Header()
    {
        var panel = NewPanel(560, 78, Background);
        panel.Controls.Add(new Label { Text = "RENDERNORTH", ForeColor = Accent, Font = new Font("Segoe UI Semibold", 9), AutoSize = true, Location = new Point(0, 0) });
        panel.Controls.Add(new Label { Text = "Display Switcher", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 24), AutoSize = true, Location = new Point(-2, 18) });
        panel.Controls.Add(new Label { Text = $"Dual-PC display routing for creators  •  v{AppVersion.Current}", ForeColor = Muted, AutoSize = true, Location = new Point(1, 59) });
        return panel;
    }

    private Control ActiveCard()
    {
        var panel = NewPanel(560, 68, Card);
        panel.Controls.Add(new Label { Text = "ACTIVE PROFILE", ForeColor = Muted, AutoSize = true, Location = new Point(18, 13) });
        _indicator.Location = new Point(20, 45); panel.Controls.Add(_indicator);
        _activeProfile.Location = new Point(42, 35); _activeProfile.AutoSize = true; panel.Controls.Add(_activeProfile);
        return panel;
    }

    private Control ModeCard(string title, string description, string buttonText, ProfileKind kind, Color color)
    {
        var panel = NewPanel(560, 94, Card);
        panel.Controls.Add(new Label { Text = title, ForeColor = color, Font = new Font("Segoe UI Semibold", 11), AutoSize = true, Location = new Point(18, 14) });
        panel.Controls.Add(new Label { Text = description, ForeColor = Muted, AutoSize = true, Location = new Point(18, 40) });
        var button = ActionButton(buttonText, color, (_, _) => RunOperation(() => _service.Activate(kind)));
        button.Location = new Point(340, 30); button.Size = new Size(195, 48); panel.Controls.Add(button);
        return panel;
    }

    private Control ProfileManagement()
    {
        var panel = NewPanel(560, 78, Card);
        panel.Controls.Add(SectionTitle("PROFILE MANAGEMENT", new Point(18, 12)));
        var game = SecondaryButton("Save Current as Game Mode", (_, _) => SaveProfile(ProfileKind.Game)); game.Location = new Point(18, 39);
        var script = SecondaryButton("Save Current as Script Mode", (_, _) => SaveProfile(ProfileKind.Script)); script.Location = new Point(286, 39);
        panel.Controls.AddRange([game, script]); return panel;
    }

    private Control Utilities()
    {
        var panel = NewPanel(560, 80, Card); panel.Controls.Add(SectionTitle("UTILITIES", new Point(18, 8)));
        var identify = SmallButton("Identify Displays", Identify);
        var logs = SmallButton("Open Logs", (_, _) => Process.Start(new ProcessStartInfo(_log.LogFolder) { UseShellExecute = true }));
        var check = SmallButton("Check for Updates", async (_, _) => await _updates.CheckAsync());
        var about = SmallButton("About", (_, _) => new AboutForm(() => _updates.CheckAsync()).ShowDialog(this));
        var controls = new[] { identify, logs, check, about };
        for (var i = 0; i < controls.Length; i++) { controls[i].Location = new Point(18 + i * 132, 34); panel.Controls.Add(controls[i]); }
        return panel;
    }

    private Control StatusCard()
    {
        var panel = NewPanel(560, 124, Card); panel.Controls.Add(SectionTitle("STATUS", new Point(18, 10)));
        _lastResult.Location = new Point(18, 39); _lastResult.Size = new Size(520, 20);
        _lastSuccessful.Location = new Point(18, 61); _lastSuccessful.Size = new Size(520, 20);
        _updateStatus.Location = new Point(18, 83); _updateStatus.Size = new Size(330, 20);
        _releaseNotes.Location = new Point(18, 104); _releaseNotes.Size = new Size(520, 18);
        _downloadButton.Location = new Point(365, 76); _downloadButton.Size = new Size(170, 34);
        panel.Controls.AddRange([_lastResult, _lastSuccessful, _updateStatus, _releaseNotes, _downloadButton]); return panel;
    }

    private void SaveProfile(ProfileKind kind)
    {
        var profileName = kind == ProfileKind.Game ? "game.json" : "script.json";
        var exists = File.Exists(Path.Combine(AppContext.BaseDirectory, "profiles", profileName));
        if (exists && MessageBox.Show(this, $"Overwrite the saved {kind} Mode profile?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        RunOperation(() => _service.Save(kind));
    }

    private void RunOperation(Func<OperationResult> operation)
    {
        UseWaitCursor = true;
        try { var result = operation(); _lastResult.Text = "Last switch result: " + OneLine(result.Message); _lastResult.ForeColor = result.Success ? Color.LightGreen : Color.Salmon; RefreshProfileStatus(); }
        finally { UseWaitCursor = false; }
    }

    private void RefreshProfileStatus()
    {
        var status = _service.GetApplicationStatus(); _activeProfile.Text = status.CurrentProfile;
        _indicator.BackColor = status.CurrentProfile switch { "Game Mode" => Color.FromArgb(35, 174, 108), "Script Mode" => Accent, "Custom / Unknown" => Color.Goldenrod, _ => Color.IndianRed };
        _lastResult.Text = "Last switch result: " + OneLine(status.LastSwitchResult);
        _lastSuccessful.Text = "Last successful switch: " + (status.LastSuccessfulSwitchAt?.ToLocalTime().ToString("g") ?? "None recorded");
    }

    private void Identify(object? sender, EventArgs args)
    {
        try { var text = _service.IdentifyDisplays(); _log.Info(text.Replace(Environment.NewLine, " | ")); MessageBox.Show(this, text, "Identify Displays"); _service.OpenWindowsIdentify(); }
        catch (Exception ex) { _log.Error("Could not identify displays", ex); _lastResult.Text = "Identify failed: " + ex.Message; }
    }

    private void OnUpdateStatusChanged(UpdateStatus status)
    {
        if (InvokeRequired) { BeginInvoke(() => OnUpdateStatusChanged(status)); return; }
        _updateStatus.Text = $"Update status: {status.Message}"; _downloadButton.Visible = status.State == UpdateState.Available;
        _releaseNotes.Text = status.State == UpdateState.Available ? OneLine(status.ReleaseNotes ?? "") : "";
    }

    private async Task DownloadAndInstallAsync()
    {
        await _updates.DownloadAsync();
        if (_updates.Status.State == UpdateState.ReadyToRestart && MessageBox.Show(this, "Update downloaded. Restart now to install it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) _updates.ApplyAndRestart();
    }

    private static Panel NewPanel(int width, int height, Color color) => new() { Width = width, Height = height, BackColor = color, Margin = new Padding(0, 0, 0, 6) };
    private static Label TextLabel(string text, float size, bool bold = false) => new() { Text = text, ForeColor = Color.White, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), AutoEllipsis = true };
    private static Label SectionTitle(string text, Point location) => new() { Text = text, ForeColor = Muted, Font = new Font("Segoe UI Semibold", 9), AutoSize = true, Location = location };
    private static Button ActionButton(string text, Color color, EventHandler action) { var b = new Button { Text = text, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; b.Click += action; return b; }
    private static Button SecondaryButton(string text, EventHandler action) { var b = ActionButton(text, Color.FromArgb(53, 64, 70), action); b.Size = new Size(256, 32); return b; }
    private static Button SmallButton(string text, EventHandler action) { var b = SecondaryButton(text, action); b.Size = new Size(122, 30); return b; }
    private static string OneLine(string value) => value.Replace("\r", " ").Replace("\n", " ");
}
