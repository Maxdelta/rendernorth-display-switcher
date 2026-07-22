using RenderNorth.DisplaySwitcher.Domain.Environments;
using RenderNorth.DisplaySwitcher.Domain.Modules;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class EnvironmentEditorForm : Form
{
    private readonly TextBox _name = new(); private readonly TextBox _description = new() { Multiline = true };
    private readonly ComboBox _icon = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDown };
    private readonly Label _displayStatus = new() { AutoSize = true };
    private readonly Func<ModuleDocument> _captureDisplays;
    public EnvironmentDefinition Environment { get; }

    public EnvironmentEditorForm(EnvironmentDefinition environment, Func<ModuleDocument> captureDisplays, bool allowDelete)
    {
        Environment = environment; _captureDisplays = captureDisplays; Text = environment.Id == Guid.Empty ? "New Environment" : "Edit Environment";
        ClientSize = new Size(500, 490); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = Color.FromArgb(24, 29, 33); ForeColor = Color.White; Font = new Font("Segoe UI", 9);
        _icon.Items.AddRange(["workspace", "gamepad", "script", "code", "work", "creative", "presentation", "travel"]);
        _category.Items.AddRange(["Gaming", "Streaming", "Development", "Work", "Creative", "Presentation", "Travel", "Custom"]);
        _name.Text = environment.Name; _description.Text = environment.Description ?? ""; _icon.SelectedItem = environment.Icon; if (_icon.SelectedIndex < 0) _icon.SelectedIndex = 0; _category.Text = environment.Category ?? "Custom";
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 12 };
        table.Controls.Add(Heading("ENVIRONMENT")); table.Controls.Add(Label("Name")); table.Controls.Add(_name); table.Controls.Add(Label("Description")); _description.Height = 70; table.Controls.Add(_description);
        table.Controls.Add(Label("Icon")); table.Controls.Add(_icon); table.Controls.Add(Label("Category")); table.Controls.Add(_category);
        table.Controls.Add(Heading("DISPLAYS"));
        var capture = ActionButton("Capture Current Display Setup", (_, _) => CaptureDisplays()); table.Controls.Add(capture);
        _displayStatus.Text = HasDisplay() ? "Current display setup captured" : "No display setup captured"; _displayStatus.ForeColor = HasDisplay() ? Color.LightGreen : Color.Goldenrod; table.Controls.Add(_displayStatus);
        var actions = new FlowLayoutPanel { Height = 48, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var save = ActionButton("Save", (_, _) => SaveAndClose()); var cancel = ActionButton("Cancel", (_, _) => { DialogResult = DialogResult.Cancel; Close(); });
        actions.Controls.Add(save); actions.Controls.Add(cancel);
        if (allowDelete) { var delete = ActionButton("Delete", (_, _) => { DialogResult = DialogResult.Abort; Close(); }); delete.BackColor = Color.FromArgb(150, 55, 55); actions.Controls.Add(delete); }
        table.Controls.Add(actions); Controls.Add(table); AcceptButton = save; CancelButton = cancel;
    }

    private void CaptureDisplays()
    {
        try { var document = _captureDisplays(); Environment.Modules.RemoveAll(module => string.Equals(module.Type, "display", StringComparison.OrdinalIgnoreCase)); Environment.Modules.Add(document); _displayStatus.Text = "Current display setup captured"; _displayStatus.ForeColor = Color.LightGreen; }
        catch (Exception exception) { MessageBox.Show(this, $"Could not capture displays.\n\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(_name.Text)) { MessageBox.Show(this, "Enter an environment name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        if (!HasDisplay()) { MessageBox.Show(this, "Capture the current display setup before saving.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Environment.Name = _name.Text.Trim(); Environment.Description = string.IsNullOrWhiteSpace(_description.Text) ? null : _description.Text.Trim(); Environment.Icon = _icon.SelectedItem?.ToString() ?? "workspace"; Environment.Category = string.IsNullOrWhiteSpace(_category.Text) ? null : _category.Text.Trim(); DialogResult = DialogResult.OK; Close();
    }
    private bool HasDisplay() => Environment.Modules.Any(module => string.Equals(module.Type, "display", StringComparison.OrdinalIgnoreCase));
    private static Label Label(string text) => new() { Text = text, ForeColor = Color.FromArgb(174, 187, 194), AutoSize = true, Margin = new Padding(0, 8, 0, 2) };
    private static Label Heading(string text) => new() { Text = text, ForeColor = Color.FromArgb(38, 198, 190), Font = new Font("Segoe UI Semibold", 10), AutoSize = true, Margin = new Padding(0, 8, 0, 4) };
    private static Button ActionButton(string text, EventHandler click) { var button = new Button { Text = text, Width = 150, Height = 34, BackColor = Color.FromArgb(53, 64, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat }; button.FlatAppearance.BorderSize = 0; button.Click += click; return button; }
}
