using System.Diagnostics;
using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class SettingsForm : Form
{
    private readonly UpdateService _updates;
    private readonly AppLogger _log;

    public SettingsForm(UpdateService updates, AppLogger log)
    {
        _updates = updates; _log = log;
        Text = "RenderNorth Environments Settings";
        ClientSize = new Size(520, 430); MinimumSize = new Size(520, 430);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent; ShowInTaskbar = false;
        BackColor = Color.FromArgb(24, 29, 33); ForeColor = Color.White; Font = new Font("Segoe UI", 9);

        var title = new Label { Text = "Settings", Font = new Font("Segoe UI Semibold", 18), AutoSize = true, Location = new Point(24, 22) };
        var intro = new Label { Text = "Configure the product experience without changing your saved environments.", ForeColor = Color.FromArgb(174, 187, 194), AutoSize = true, Location = new Point(26, 58) };
        var sections = new TableLayoutPanel { Location = new Point(24, 96), Size = new Size(472, 260), ColumnCount = 1, RowCount = 5, BackColor = Color.FromArgb(35, 42, 47), Padding = new Padding(18) };
        sections.RowStyles.Clear(); for (var i = 0; i < 5; i++) sections.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        sections.Controls.Add(Section("GENERAL", "RenderNorth Environments runs locally on this Windows account."));
        sections.Controls.Add(Section("UPDATES", "Installed edition checks GitHub Releases. Portable edition requires manual updates."));
        sections.Controls.Add(Section("LOGGING", $"Logs are stored locally in {_log.LogFolder}."));
        sections.Controls.Add(Section("DIAGNOSTICS", "Use Identify Displays from the main window to review connected monitor identities."));
        var shortcuts = new Button { Text = "Open Shortcut Folder", AutoSize = true, BackColor = Color.FromArgb(53, 64, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Right };
        shortcuts.FlatAppearance.BorderSize = 0; shortcuts.Click += (_, _) => OpenShortcutFolder();
        var shortcutPanel = new Panel { Dock = DockStyle.Fill }; shortcutPanel.Controls.Add(Section("SHORTCUT MANAGEMENT", "Environment shortcuts remain stable across updates and renames.")); shortcuts.Location = new Point(300, 9); shortcutPanel.Controls.Add(shortcuts); sections.Controls.Add(shortcutPanel);
        var about = new Button { Text = "About", Location = new Point(24, 374), Size = new Size(100, 32), BackColor = Color.FromArgb(53, 64, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        about.FlatAppearance.BorderSize = 0; about.Click += (_, _) => new AboutForm(() => _updates.CheckAsync()).ShowDialog(this);
        var close = new Button { Text = "Close", DialogResult = DialogResult.OK, Location = new Point(396, 374), Size = new Size(100, 32), BackColor = Color.FromArgb(38, 198, 190), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        close.FlatAppearance.BorderSize = 0;
        Controls.AddRange([title, intro, sections, about, close]); AcceptButton = close; CancelButton = close;
    }

    private static Label Section(string heading, string body) => new() { Text = $"{heading}\n{body}", ForeColor = Color.White, Dock = DockStyle.Fill, AutoEllipsis = true, Padding = new Padding(0, 4, 0, 0) };

    private void OpenShortcutFolder()
    {
        try { Process.Start(new ProcessStartInfo(Environment.GetFolderPath(Environment.SpecialFolder.Programs)) { UseShellExecute = true }); }
        catch (Exception exception) { _log.Error("Could not open the shortcut folder from Settings.", exception); MessageBox.Show(this, exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
