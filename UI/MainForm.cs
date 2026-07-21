using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class MainForm : Form
{
    private readonly DisplayProfileService _service;
    private readonly AppLogger _log;
    private readonly Label _currentProfile = new();
    private readonly Label _lastResult = new();
    private readonly Label _lastSuccessfulSwitch = new();

    public MainForm(DisplayProfileService service, AppLogger log)
    {
        _service = service;
        _log = log;
        Text = "RenderNorth Display Switcher";
        ClientSize = new Size(430, 529);
        MinimumSize = new Size(446, 568);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label { Text = "RenderNorth Display Switcher", Font = new Font(Font.FontFamily, 15, FontStyle.Bold), AutoSize = true, Margin = new Padding(3, 3, 3, 12) };
        var note = new Label { Text = "Save each layout after configuring it in Windows Display Settings.", AutoSize = true, MaximumSize = new Size(390, 0), Margin = new Padding(3, 0, 3, 14) };
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(18), AutoScroll = true };
        panel.Controls.Add(title); panel.Controls.Add(note);
        panel.Controls.Add(Button("Save Current Layout as Game Mode", (_, _) => RunOperation(() => _service.Save(ProfileKind.Game))));
        panel.Controls.Add(Button("Save Current Layout as Script Mode", (_, _) => RunOperation(() => _service.Save(ProfileKind.Script))));
        panel.Controls.Add(Button("Activate Game Mode", (_, _) => RunOperation(() => _service.Activate(ProfileKind.Game))));
        panel.Controls.Add(Button("Activate Script Mode", (_, _) => RunOperation(() => _service.Activate(ProfileKind.Script))));
        panel.Controls.Add(Button("Identify Displays", Identify));
        panel.Controls.Add(Button("About", (_, _) => new AboutForm().ShowDialog(this)));
        panel.Controls.Add(CreateStatusPanel());
        Controls.Add(panel);
        Shown += (_, _) => RefreshStatus();
    }

    private static Button Button(string text, EventHandler action)
    {
        var button = new Button { Text = text, Width = 385, Height = 39, Margin = new Padding(3, 3, 3, 5), UseVisualStyleBackColor = true };
        button.Click += action;
        return button;
    }

    private void Identify(object? sender, EventArgs args)
    {
        try
        {
            var description = _service.IdentifyDisplays();
            _log.Info(description.Replace(Environment.NewLine, " | "));
            MessageBox.Show(this, description, "Identify Displays", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _service.OpenWindowsIdentify();
        }
        catch (Exception ex)
        {
            _log.Error("Could not identify displays", ex);
            MessageBox.Show(this, $"Could not identify displays.\n\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private Control CreateStatusPanel()
    {
        var box = new GroupBox { Text = "Status", Width = 385, Height = 135, Margin = new Padding(3, 12, 3, 3) };
        _currentProfile.SetBounds(12, 25, 355, 20);
        _lastResult.SetBounds(12, 50, 355, 38);
        _lastResult.AutoEllipsis = true;
        _lastSuccessfulSwitch.SetBounds(12, 95, 355, 20);
        box.Controls.AddRange([_currentProfile, _lastResult, _lastSuccessfulSwitch]);
        return box;
    }

    private void RunOperation(Func<OperationResult> operation)
    {
        UseWaitCursor = true;
        try
        {
            var result = operation();
            RefreshStatus(result.Message, result.Success);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void RefreshStatus(string? immediateResult = null, bool? success = null)
    {
        var status = _service.GetApplicationStatus();
        _currentProfile.Text = $"Current detected profile: {status.CurrentProfile}";
        _currentProfile.Font = new Font(Font, FontStyle.Bold);
        _lastResult.Text = $"Last switch result: {immediateResult ?? OneLine(status.LastSwitchResult)}";
        _lastResult.ForeColor = success switch { true => Color.DarkGreen, false => Color.DarkRed, _ => SystemColors.ControlText };
        _lastSuccessfulSwitch.Text = "Last successful switch: " +
            (status.LastSuccessfulSwitchAt?.ToLocalTime().ToString("g") ?? "None recorded");
    }

    private static string OneLine(string value) => value.Replace("\r", " ").Replace("\n", " ");
}
