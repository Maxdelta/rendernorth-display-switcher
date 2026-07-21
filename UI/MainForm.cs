using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class MainForm : Form
{
    private readonly DisplayProfileService _service;
    private readonly AppLogger _log;

    public MainForm(DisplayProfileService service, AppLogger log)
    {
        _service = service;
        _log = log;
        Text = "RenderNorth Display Switcher";
        ClientSize = new Size(430, 330);
        MinimumSize = new Size(446, 369);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label { Text = "RenderNorth Display Switcher", Font = new Font(Font.FontFamily, 15, FontStyle.Bold), AutoSize = true, Margin = new Padding(3, 3, 3, 12) };
        var note = new Label { Text = "Save each layout after configuring it in Windows Display Settings.", AutoSize = true, MaximumSize = new Size(390, 0), Margin = new Padding(3, 0, 3, 14) };
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(18), AutoScroll = true };
        panel.Controls.Add(title); panel.Controls.Add(note);
        panel.Controls.Add(Button("Save Current Layout as Game Mode", (_, _) => Show(_service.Save(ProfileKind.Game))));
        panel.Controls.Add(Button("Save Current Layout as Script Mode", (_, _) => Show(_service.Save(ProfileKind.Script))));
        panel.Controls.Add(Button("Activate Game Mode", (_, _) => Show(_service.Activate(ProfileKind.Game))));
        panel.Controls.Add(Button("Activate Script Mode", (_, _) => Show(_service.Activate(ProfileKind.Script))));
        panel.Controls.Add(Button("Identify Displays", Identify));
        Controls.Add(panel);
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

    private void Show(OperationResult result) => MessageBox.Show(this, result.Message, Text, MessageBoxButtons.OK,
        result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
}
