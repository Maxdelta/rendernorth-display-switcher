using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class MainForm : Form
{
    private static readonly Color Background = RnTheme.Background;
    private static readonly Color Card = RnTheme.Card;
    private static readonly Color Accent = RnTheme.Accent;
    private static readonly Color Gaming = Color.FromArgb(155, 109, 255);
    private static readonly Color Streaming = Color.FromArgb(240, 93, 103);
    private static readonly Color Development = Color.FromArgb(76, 154, 255);
    private static readonly Color Presentation = Color.FromArgb(76, 203, 138);
    private static readonly Color Muted = RnTheme.SecondaryText;
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
    private Guid _detectedEnvironmentId;

    public MainForm(EnvironmentManager manager, UpdateService updates, AppLogger log,
        Func<ModuleDocument> captureDisplays, Func<string> identifyDisplays, Action openDisplaySettings, ShortcutService shortcuts)
    {
        _manager = manager; _updates = updates; _log = log; _captureDisplays = captureDisplays;
        _identifyDisplays = identifyDisplays; _openDisplaySettings = openDisplaySettings; _shortcuts = shortcuts;
        Text = "RenderNorth Environments"; ClientSize = new Size(900, 760); MinimumSize = new Size(620, 520);
        StartPosition = FormStartPosition.CenterScreen; BackColor = Background; ForeColor = RnTheme.PrimaryText;
        Font = new Font("Segoe UI", 9); AutoScaleMode = AutoScaleMode.Dpi;

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(LayoutTokens.WindowInset), BackColor = Background, AutoSize = false };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(Header(), 0, 0); root.Controls.Add(CurrentCard(), 0, 1);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Background, Padding = new Padding(0, LayoutTokens.SectionGap, 0, LayoutTokens.SectionGap) };
        var content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 2, BackColor = Background };
        _environmentList.Dock = DockStyle.Top; _environmentList.AutoSize = true; _environmentList.AutoSizeMode = AutoSizeMode.GrowAndShrink; _environmentList.BackColor = Background; _environmentList.Padding = new Padding(0, 4, 0, 4);
        content.Controls.Add(_environmentList, 0, 0); content.Controls.Add(Actions(), 0, 1); scroll.Controls.Add(content); root.Controls.Add(scroll, 0, 2);
        _downloadButton = Button("Download and Install", Accent, async (_, _) => await DownloadAndInstallAsync()); _downloadButton.Visible = false;
        root.Controls.Add(StatusCard(), 0, 3); Controls.Add(root);
        _updates.StatusChanged += OnUpdateStatusChanged;
        Shown += async (_, _) => { await RefreshAsync(); await _updates.CheckAsync(); };
    }

    private Control Header() => new RnHeader(AppVersion.Current, (_, _) => new SettingsForm(_updates, _log).ShowDialog(this), async (_, _) => await _updates.CheckAsync());
    /*
    {
        var panel = Panel(Card);
        panel.Controls.Add(new Label { Text = "✦  RENDERNORTH", ForeColor = Accent, Font = new Font("Segoe UI Semibold", 9), AutoSize = true, Location = new Point(18, 12) });
        panel.Controls.Add(new Label { Text = "Environments", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 25), AutoSize = true, Location = new Point(16, 29) });
        panel.Controls.Add(new Label { Text = $"Your PC should adapt to what you are doing  •  v{AppVersion.Current}", ForeColor = Muted, AutoSize = true, Location = new Point(310, 49) });
        return panel;
    } */

    private Control CurrentCard() => new RnHeroCard("Custom Configuration", "No saved environment matches the current display setup.", false, null, () => EditNew(true));
    /*
    {
        var panel = Panel(Card); panel.Margin = new Padding(0, 6, 0, 6);
        panel.Controls.Add(new Label { Text = "✦  ACTIVE ENVIRONMENT", ForeColor = Accent, AutoSize = true, Location = new Point(18, 13) });
        _currentName.Location = new Point(18, 36); _currentName.Size = new Size(680, 32); _currentName.Font = new Font("Segoe UI Semibold", 21); panel.Controls.Add(_currentName);
        _currentDetails.Location = new Point(20, 78); _currentDetails.Size = new Size(680, 22); _currentDetails.ForeColor = Muted; panel.Controls.Add(_currentDetails);
        panel.Controls.Add(new Label { Text = "Ready to adapt your workspace", ForeColor = Color.LightGreen, AutoSize = true, Location = new Point(560, 18) }); return panel;
    } */

    private Control Actions() => new RnCreatePanel((_,_) => EditNew(false), (_,_) => EditNew(true), Identify, (_,_) => new SettingsForm(_updates, _log).ShowDialog(this));
    /*
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
    } */

    private Control StatusCard() => new RnStatusBar(_lastResult.Text, _lastSuccessful.Text, _updateStatus.Text);
    /*
    {
        var panel = Panel(Card); panel.Margin = new Padding(0, 6, 0, 0);
        panel.Controls.Add(new Label { Text = "STATUS", ForeColor = Muted, AutoSize = true, Location = new Point(18, 12) });
        _lastResult.Location = new Point(18, 34); _lastResult.Size = new Size(680, 20);
        _lastSuccessful.Location = new Point(18, 55); _lastSuccessful.Size = new Size(680, 20);
        _updateStatus.Location = new Point(18, 78); _updateStatus.Size = new Size(430, 20);
        _downloadButton.Location = new Point(510, 67); _downloadButton.Size = new Size(190, 34);
        panel.Controls.AddRange([_lastResult, _lastSuccessful, _updateStatus, _downloadButton]); return panel;
    } */

    private async Task RefreshAsync()
    {
        EnvironmentCollection collection;
        try { collection = _manager.Load(); }
        catch (Exception exception) { _log.Error("Could not load environments", exception); _lastResult.Text = "Could not load environments: " + exception.Message; return; }
        _environmentList.SuspendLayout(); _environmentList.Controls.Clear();
        foreach (var environment in collection.Environments.OrderBy(item => item.SortOrder).ThenBy(item => item.Name)) _environmentList.Controls.Add(EnvironmentCard(environment));
        if (collection.Environments.Count == 0)
        {
            _environmentList.Controls.Add(EmptyState());
        }
        _environmentList.ResumeLayout(); _lastResult.Text = "Last activation: " + collection.ActivationStatus.LastResult;
        _lastSuccessful.Text = "Last successful activation: " + (collection.ActivationStatus.LastSuccessfulAt?.ToLocalTime().ToString("g") ?? "None recorded");
        try
        {
            var detected = await _manager.DetectAsync();
            _detectedEnvironmentId = detected.Environment?.Id ?? Guid.Empty;
            _currentName.Text = detected.Environment?.Name ?? "Custom Configuration";
            _currentDetails.Text = detected.Environment is null ? "No matching environment" : $"{IconLabel(detected.Environment.Icon)}  {detected.Environment.Category ?? "Custom"}";
        }
        catch (Exception exception) { _log.Error("Environment detection failed", exception); _currentName.Text = "Custom Configuration"; _currentDetails.Text = "Detection unavailable"; }
    }

    private Control EnvironmentCard(EnvironmentDefinition environment)
    {
        var panel = Panel(Card); panel.Dock = DockStyle.None; panel.Width = Math.Max(680, _environmentList.ClientSize.Width - 24); panel.Height = 126; panel.Margin = new Padding(0, 0, 0, 9);
        var accent = CategoryColor(environment.Category); panel.Controls.Add(new Panel { BackColor = accent, Width = 5, Dock = DockStyle.Left });
        var icon = LabelText(IconLabel(environment.Icon), 22, true); icon.BackColor = Color.FromArgb(45, accent.R, accent.G, accent.B); icon.Location = new Point(20, 25); icon.Size = new Size(58, 58); icon.TextAlign = ContentAlignment.MiddleCenter;
        var name = LabelText(environment.Name, 13, true); name.Location = new Point(72, 13); name.Size = new Size(330, 25);
        var details = LabelText($"{environment.Category ?? "Custom"}  •  {environment.Description ?? "Display workspace"}", 9); details.ForeColor = Muted; details.Location = new Point(73, 42); details.Size = new Size(350, 35);
        var preview = new RnDisplayPreview { Category = environment.Category ?? "Custom", Location = new Point(300, 76) }; panel.Controls.Add(preview);
        var activate = Button("Activate", Accent, async (_, _) => await ActivateAsync(environment.Id)); activate.Location = new Point(430, 35); activate.Size = new Size(112, 42);
        var shortcut = Button("Stream Deck", accent, (_, _) => CreateShortcut(() => _shortcuts.CreateStartMenu(environment))); shortcut.Location = new Point(550, 35); shortcut.Size = new Size(102, 42);
        var edit = Button("•••", Color.FromArgb(53, 64, 70), (_, _) => Edit(environment)); edit.Location = new Point(660, 35); edit.Size = new Size(42, 42);
        Button? more = null;
        more = Button("Manage", Color.FromArgb(53, 64, 70), (_, _) => ShowMore(environment, more!)); more.Location = new Point(660, 82); more.Size = new Size(74, 28);
        panel.Controls.AddRange([icon, name, details, preview, activate, shortcut, edit, more]); return panel;
    }

    private Control EmptyState() => new RnEmptyState(() => EditNew(true), () => EditNew(false));
    /*
    {
        var panel = new RnCard { Width = Math.Max(680, _environmentList.ClientSize.Width - 24), Height = 270, BackColor = Card, Margin = new Padding(0, 0, 0, 9), Padding = new Padding(8) };
        var star = LabelText("✦", 24, true); star.ForeColor = Accent; star.Location = new Point(30, 26); star.AutoSize = true;
        var title = LabelText("Welcome to RenderNorth Environments", 18, true); title.Location = new Point(70, 26); title.AutoSize = true;
        var subtitle = LabelText("Your PC should adapt to what you're doing.\nSave your current display setup to create your first environment.", 10); subtitle.ForeColor = Muted; subtitle.Location = new Point(72, 60); subtitle.Size = new Size(560, 38);
        panel.Controls.AddRange([star, title, subtitle]);
        var ideas = new[] { ("🎮", "Gaming", Gaming), ("💻", "Development", Development), ("🎥", "Streaming", Streaming), ("📺", "Presentation", Presentation) };
        for (var i = 0; i < ideas.Length; i++)
        {
            var (icon, name, color) = ideas[i]; var card = new RnCard { BackColor = Color.FromArgb(44, 51, 57), BorderColor = Color.FromArgb(70, 80, 88), Location = new Point(30 + i * 156, 116), Size = new Size(142, 72), Cursor = Cursors.Hand, Padding = new Padding(4) };
            var glyph = LabelText(icon, 18, true); glyph.Location = new Point(16, 18); glyph.AutoSize = true; var label = LabelText(name, 9, true); label.ForeColor = color; label.Location = new Point(52, 31); label.AutoSize = true; card.Controls.AddRange([glyph, label]); card.Click += (_, _) => EditNew(true); glyph.Click += (_, _) => EditNew(true); label.Click += (_, _) => EditNew(true); panel.Controls.Add(card);
        }
        var hint = LabelText("Capture a setup to get started, or create an environment from scratch.", 9); hint.ForeColor = Muted; hint.Location = new Point(30, 210); hint.AutoSize = true; panel.Controls.Add(hint);
        return panel;
    } */

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
    private static Panel Panel(Color color) => new RnCard { Dock = DockStyle.Fill, BackColor = color };
    private static Label LabelText(string text, float size, bool bold = false) => new() { Text = text, ForeColor = RnTheme.PrimaryText, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), AutoEllipsis = true };
    private static Button Button(string text, Color color, EventHandler action) { var button = new RnButton(color) { Text = text }; button.Click += action; return button; }
    private static string IconLabel(string icon) => icon.ToLowerInvariant() switch { "gamepad" => "🎮", "script" => "🎥", "code" => "💻", "work" => "🧰", "creative" => "🎨", "presentation" => "📺", "travel" => "✈", "camera" => "📷", "microphone" => "🎙", "monitor" => "🖥", _ => "◆" };
    private static Color CategoryColor(string? category) => category?.ToLowerInvariant() switch { "gaming" => Gaming, "streaming" => Streaming, "development" => Development, "presentation" => Presentation, _ => Accent };
    private static string Preview(string? category) => category?.ToLowerInvariant() switch { "gaming" => "▣══▣  3 displays  •  Elgato ready", "streaming" => "▣──▣  3 displays  •  Capture ready", "development" => "▣  ▣  ▣  3 displays  •  Extended", _ => "▣  ▣  Display setup saved" };
    private static EnvironmentDefinition Clone(EnvironmentDefinition source) => new() { Id = source.Id, Name = source.Name, Description = source.Description, Icon = source.Icon, Category = source.Category, Accent = source.Accent, Tags = [.. source.Tags], CreatedAt = source.CreatedAt, UpdatedAt = source.UpdatedAt, IsFavorite = source.IsFavorite, SortOrder = source.SortOrder, LegacyAliases = [.. source.LegacyAliases], Modules = source.Modules.Select(module => module.Clone()).ToList(), Metadata = new Dictionary<string, string>(source.Metadata, StringComparer.OrdinalIgnoreCase) };
}
