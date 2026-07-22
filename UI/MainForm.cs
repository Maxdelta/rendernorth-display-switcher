using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class MainForm : Form
{
    private static readonly Color Background = Color.FromArgb(24, 29, 33);
    private static readonly Color Card = Color.FromArgb(35, 42, 47);
    private static readonly Color Accent = Color.FromArgb(38, 198, 190);
    private static readonly Color Muted = Color.FromArgb(174, 187, 194);
    private readonly EnvironmentManager _manager;
    private readonly UpdateService _updates;
    private readonly AppLogger _log;
    private readonly Func<ModuleDocument> _captureDisplays;
    private readonly Func<string> _identifyDisplays;
    private readonly Action _openDisplaySettings;
    private readonly ShortcutService _shortcuts;
    private readonly FlowLayoutPanel _environmentList = new() { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
    private readonly Label _currentName = LabelText("Custom Configuration", 18, true);
    private readonly Label _currentDetails = LabelText("No matching environment", 9);
    private readonly Label _lastResult = LabelText("No environment activation has been recorded yet.", 9);
    private readonly Label _lastSuccessful = LabelText("None recorded", 9);
    private readonly Label _updateStatus = LabelText("Not checked", 9);
    private readonly Button _downloadButton;

    public MainForm(EnvironmentManager manager, UpdateService updates, AppLogger log,
        Func<ModuleDocument> captureDisplays, Func<string> identifyDisplays, Action openDisplaySettings, ShortcutService shortcuts)
    {
        _manager = manager; _updates = updates; _log = log; _captureDisplays = captureDisplays;
        _identifyDisplays = identifyDisplays; _openDisplaySettings = openDisplaySettings; _shortcuts = shortcuts;
        Text = "RenderNorth Environments"; ClientSize = new Size(760, 780); MinimumSize = new Size(776, 700);
        StartPosition = FormStartPosition.CenterScreen; BackColor = Background; ForeColor = Color.White;
        Font = new Font("Segoe UI", 9); AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(18), BackColor = Background };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        root.Controls.Add(Header(), 0, 0); root.Controls.Add(CurrentCard(), 0, 1);
        _environmentList.Dock = DockStyle.Fill; _environmentList.BackColor = Background; _environmentList.Padding = new Padding(0, 4, 0, 4);
        root.Controls.Add(_environmentList, 0, 2); root.Controls.Add(Actions(), 0, 3);
        _downloadButton = Button("Download and Install", Accent, async (_, _) => await DownloadAndInstallAsync()); _downloadButton.Visible = false;
        root.Controls.Add(StatusCard(), 0, 4); Controls.Add(root);
        _updates.StatusChanged += OnUpdateStatusChanged;
        Shown += async (_, _) => { await RefreshAsync(); await _updates.CheckAsync(); };
    }

    private Control Header()
    {
        var panel = Panel(Card);
        panel.Controls.Add(new Label { Text = "RENDERNORTH", ForeColor = Accent, Font = new Font("Segoe UI Semibold", 9), AutoSize = true, Location = new Point(18, 12) });
        panel.Controls.Add(new Label { Text = "Environments", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 25), AutoSize = true, Location = new Point(16, 29) });
        panel.Controls.Add(new Label { Text = $"Your PC should adapt to what you are doing  •  v{AppVersion.Current}", ForeColor = Muted, AutoSize = true, Location = new Point(310, 49) });
        return panel;
    }

    private Control CurrentCard()
    {
        var panel = Panel(Card); panel.Margin = new Padding(0, 6, 0, 6);
        panel.Controls.Add(new Label { Text = "CURRENT ENVIRONMENT", ForeColor = Muted, AutoSize = true, Location = new Point(18, 13) });
        _currentName.Location = new Point(18, 35); _currentName.Size = new Size(680, 30); panel.Controls.Add(_currentName);
        _currentDetails.Location = new Point(20, 70); _currentDetails.Size = new Size(680, 22); panel.Controls.Add(_currentDetails); return panel;
    }

    private Control Actions()
    {
        var panel = Panel(Background); var actions = new[]
        {
            Button("New Environment", Accent, (_, _) => EditNew(false)),
            Button("Capture Current Setup", Color.FromArgb(35, 174, 108), (_, _) => EditNew(true)),
            Button("Identify Displays", Color.FromArgb(53, 64, 70), Identify),
            Button("Settings", Color.FromArgb(53, 64, 70), (_, _) => new SettingsForm(_updates, _log).ShowDialog(this))
        };
        for (var index = 0; index < actions.Length; index++) { actions[index].Size = new Size(166, 38); actions[index].Location = new Point(index * 174, 8); panel.Controls.Add(actions[index]); }
        return panel;
    }

    private Control StatusCard()
    {
        var panel = Panel(Card); panel.Margin = new Padding(0, 6, 0, 0);
        panel.Controls.Add(new Label { Text = "STATUS", ForeColor = Muted, AutoSize = true, Location = new Point(18, 12) });
        _lastResult.Location = new Point(18, 34); _lastResult.Size = new Size(680, 20);
        _lastSuccessful.Location = new Point(18, 55); _lastSuccessful.Size = new Size(680, 20);
        _updateStatus.Location = new Point(18, 78); _updateStatus.Size = new Size(430, 20);
        _downloadButton.Location = new Point(510, 67); _downloadButton.Size = new Size(190, 34);
        panel.Controls.AddRange([_lastResult, _lastSuccessful, _updateStatus, _downloadButton]); return panel;
    }

    private async Task RefreshAsync()
    {
        EnvironmentCollection collection;
        try { collection = _manager.Load(); }
        catch (Exception exception) { _log.Error("Could not load environments", exception); _lastResult.Text = "Could not load environments: " + exception.Message; return; }
        _environmentList.SuspendLayout(); _environmentList.Controls.Clear();
        foreach (var environment in collection.Environments.OrderBy(item => item.SortOrder).ThenBy(item => item.Name)) _environmentList.Controls.Add(EnvironmentCard(environment));
        if (collection.Environments.Count == 0)
        {
            var empty = LabelText("No environments yet. Capture the current setup to create your first workspace.", 11);
            empty.ForeColor = Muted; empty.Size = new Size(690, 70); empty.TextAlign = ContentAlignment.MiddleCenter; _environmentList.Controls.Add(empty);
        }
        _environmentList.ResumeLayout(); _lastResult.Text = "Last activation: " + collection.ActivationStatus.LastResult;
        _lastSuccessful.Text = "Last successful activation: " + (collection.ActivationStatus.LastSuccessfulAt?.ToLocalTime().ToString("g") ?? "None recorded");
        try
        {
            var detected = await _manager.DetectAsync();
            _currentName.Text = detected.Environment?.Name ?? "Custom Configuration";
            _currentDetails.Text = detected.Environment is null ? "No matching environment" : $"{IconLabel(detected.Environment.Icon)}  {detected.Environment.Category ?? "Custom"}";
        }
        catch (Exception exception) { _log.Error("Environment detection failed", exception); _currentName.Text = "Custom Configuration"; _currentDetails.Text = "Detection unavailable"; }
    }

    private Control EnvironmentCard(EnvironmentDefinition environment)
    {
        var panel = Panel(Card); panel.Dock = DockStyle.None; panel.Width = Math.Max(680, _environmentList.ClientSize.Width - 24); panel.Height = 92; panel.Margin = new Padding(0, 0, 0, 7);
        var icon = LabelText(IconLabel(environment.Icon), 18, true); icon.Location = new Point(16, 26); icon.Size = new Size(48, 42); icon.TextAlign = ContentAlignment.MiddleCenter;
        var name = LabelText(environment.Name, 13, true); name.Location = new Point(72, 13); name.Size = new Size(330, 25);
        var details = LabelText($"{environment.Category ?? "Custom"}  •  {environment.Description ?? "Display workspace"}", 9); details.ForeColor = Muted; details.Location = new Point(73, 42); details.Size = new Size(350, 35);
        var activate = Button("Activate", Accent, async (_, _) => await ActivateAsync(environment.Id)); activate.Location = new Point(430, 24); activate.Size = new Size(112, 42);
        var edit = Button("Edit", Color.FromArgb(53, 64, 70), (_, _) => Edit(environment)); edit.Location = new Point(550, 24); edit.Size = new Size(74, 42);
        Button? more = null;
        more = Button("•••", Color.FromArgb(53, 64, 70), (_, _) => ShowMore(environment, more!)); more.Location = new Point(632, 24); more.Size = new Size(48, 42);
        panel.Controls.AddRange([icon, name, details, activate, edit, more]); return panel;
    }

    private async Task ActivateAsync(Guid id)
    {
        UseWaitCursor = true; _environmentList.Enabled = false;
        try { var result = await _manager.ActivateAsync(id); _lastResult.Text = "Last activation: " + result.Message; _lastResult.ForeColor = result.Success ? Color.LightGreen : Color.Salmon; await RefreshAsync(); }
        finally { _environmentList.Enabled = true; UseWaitCursor = false; }
    }

    private void EditNew(bool capture)
    {
        var draft = new EnvironmentDefinition { Id = Guid.Empty, Name = "New Environment" };
        if (capture) try { draft.Modules = [_captureDisplays()]; } catch (Exception exception) { ShowError("Could not capture the current display setup.", exception); return; }
        using var editor = new EnvironmentEditorForm(draft, _captureDisplays, false);
        if (editor.ShowDialog(this) != DialogResult.OK) return;
        var result = _manager.Create(editor.Environment.Name, editor.Environment.Description, editor.Environment.Icon, editor.Environment.Category, editor.Environment.Modules);
        if (!result.Success) MessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); else _ = RefreshAsync();
    }

    private void Edit(EnvironmentDefinition environment)
    {
        using var editor = new EnvironmentEditorForm(Clone(environment), _captureDisplays, true);
        var dialog = editor.ShowDialog(this);
        if (dialog == DialogResult.Abort) { Delete(environment); return; }
        if (dialog != DialogResult.OK) return;
        var result = _manager.Update(editor.Environment);
        if (!result.Success) MessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); else _ = RefreshAsync();
    }

    private void ShowMore(EnvironmentDefinition environment, Control owner)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Duplicate", null, (_, _) => { var result = _manager.Duplicate(environment.Id); if (!result.Success) ShowWarning(result.Message); _ = RefreshAsync(); });
        menu.Items.Add("Move Up", null, (_, _) => { _manager.Move(environment.Id, -1); _ = RefreshAsync(); });
        menu.Items.Add("Move Down", null, (_, _) => { _manager.Move(environment.Id, 1); _ = RefreshAsync(); });
        var createShortcut = new ToolStripMenuItem("Create Shortcut");
        createShortcut.DropDownItems.Add("Desktop", null, (_, _) => CreateShortcut(() => _shortcuts.CreateDesktop(environment)));
        createShortcut.DropDownItems.Add("Start Menu", null, (_, _) => CreateShortcut(() => _shortcuts.CreateStartMenu(environment)));
        createShortcut.DropDownItems.Add("Custom Folder...", null, (_, _) => CreateCustomShortcut(environment));
        menu.Items.Add(createShortcut);
        menu.Items.Add("Copy Launch Command", null, (_, _) => { try { Clipboard.SetText(_shortcuts.LaunchCommand(environment)); _lastResult.Text = "Launch command copied."; } catch (Exception exception) { ShowError("Could not copy the launch command.", exception); } });
        menu.Items.Add("Open Shortcut Folder", null, (_, _) => OpenShortcutFolder(environment));
        menu.Items.Add("Recreate Shortcuts", null, (_, _) => CreateShortcut(() => { var paths = _shortcuts.Recreate(environment); return paths.Count == 0 ? throw new InvalidOperationException("No managed shortcuts exist for this environment.") : string.Join(Environment.NewLine, paths); }));
        menu.Items.Add("Delete Managed Shortcuts", null, (_, _) => { try { var count = _shortcuts.DeleteManaged(environment.Id); _lastResult.Text = $"Deleted {count} managed shortcut(s)."; } catch (Exception exception) { ShowError("Could not delete managed shortcuts.", exception); } });
        menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("Delete", null, (_, _) => Delete(environment)); menu.Show(owner, new Point(0, owner.Height));
    }

    private void CreateCustomShortcut(EnvironmentDefinition environment)
    {
        using var dialog = new FolderBrowserDialog { Description = "Choose where to create the environment shortcut", UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) CreateShortcut(() => _shortcuts.CreateCustom(environment, dialog.SelectedPath));
    }

    private void CreateShortcut(Func<string> create)
    {
        try { var path = create(); _lastResult.Text = "Shortcut ready: " + path; }
        catch (Exception exception) { ShowError("Could not create the environment shortcut.", exception); }
    }

    private void OpenShortcutFolder(EnvironmentDefinition environment)
    {
        try
        {
            var folder = _shortcuts.ManagedFolders(environment.Id).FirstOrDefault();
            if (folder is null) throw new InvalidOperationException("No managed shortcuts exist for this environment.");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(folder) { UseShellExecute = true });
        }
        catch (Exception exception) { ShowError("Could not open the shortcut folder.", exception); }
    }

    private void Delete(EnvironmentDefinition environment)
    {
        if (MessageBox.Show(this, $"Delete '{environment.Name}'?\n\nExisting shortcuts for this environment will stop working.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var removeShortcuts = MessageBox.Show(this, "Also delete managed Desktop and Start menu shortcuts?\n\nUnmanaged shortcuts will not be touched.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        var result = _manager.Delete(environment.Id);
        if (!result.Success) { ShowWarning(result.Message); return; }
        if (removeShortcuts) try { _shortcuts.DeleteManaged(environment.Id); } catch (Exception exception) { ShowError("The environment was deleted, but managed shortcuts could not be removed.", exception); }
        _ = RefreshAsync();
    }

    private void Identify(object? sender, EventArgs args)
    {
        try { var text = _identifyDisplays(); _log.Info(text.Replace(Environment.NewLine, " | ")); MessageBox.Show(this, text, "Identify Displays"); _openDisplaySettings(); }
        catch (Exception exception) { ShowError("Could not identify displays.", exception); }
    }

    private async Task DownloadAndInstallAsync()
    {
        await _updates.DownloadAsync();
        if (_updates.Status.State == UpdateState.ReadyToRestart && MessageBox.Show(this, "Update downloaded. Restart now to install it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) _updates.ApplyAndRestart();
    }
    private void OnUpdateStatusChanged(UpdateStatus status) { if (InvokeRequired) { BeginInvoke(() => OnUpdateStatusChanged(status)); return; } _updateStatus.Text = "Update status: " + status.Message; _downloadButton.Visible = status.State == UpdateState.Available; }
    private void ShowError(string message, Exception exception) { _log.Error(message, exception); MessageBox.Show(this, $"{message}\n\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    private void ShowWarning(string message) => MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    private static Panel Panel(Color color) => new() { Dock = DockStyle.Fill, BackColor = color };
    private static Label LabelText(string text, float size, bool bold = false) => new() { Text = text, ForeColor = Color.White, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), AutoEllipsis = true };
    private static Button Button(string text, Color color, EventHandler action) { var button = new Button { Text = text, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; button.FlatAppearance.BorderSize = 0; button.Click += action; return button; }
    private static string IconLabel(string icon) => icon.ToLowerInvariant() switch { "gamepad" => "🎮", "script" => "🎥", "code" => "💻", "work" => "🧰", "creative" => "🎨", "presentation" => "📺", "travel" => "✈", "camera" => "📷", "microphone" => "🎙", "monitor" => "🖥", _ => "◆" };
    private static EnvironmentDefinition Clone(EnvironmentDefinition source) => new() { Id = source.Id, Name = source.Name, Description = source.Description, Icon = source.Icon, Category = source.Category, Accent = source.Accent, Tags = [.. source.Tags], CreatedAt = source.CreatedAt, UpdatedAt = source.UpdatedAt, IsFavorite = source.IsFavorite, SortOrder = source.SortOrder, LegacyAliases = [.. source.LegacyAliases], Modules = source.Modules.Select(module => module.Clone()).ToList(), Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.OrdinalIgnoreCase) };
}
