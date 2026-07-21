using System.Diagnostics;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class AboutForm : Form
{
    private const string GitHubUrl = "https://github.com/RenderNorth/rendernorth-display-switcher";
    private const string WebsiteUrl = "https://www.rendernorth.com/";

    public AboutForm()
    {
        Text = "About RenderNorth Display Switcher";
        ClientSize = new Size(370, 245);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var title = new Label
        {
            Text = "RenderNorth Display Switcher",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(30, 25)
        };
        var version = new Label { Text = "Version 0.2.0", AutoSize = true, Location = new Point(31, 65) };
        var creator = new Label { Text = "Created by RenderNorth", AutoSize = true, Location = new Point(31, 92) };
        var github = Link("GitHub", GitHubUrl, new Point(31, 128));
        var website = Link("Website", WebsiteUrl, new Point(105, 128));
        var updates = new Button
        {
            Text = "Check for Updates (future)",
            Enabled = false,
            Location = new Point(31, 164),
            Size = new Size(205, 32)
        };
        var close = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.OK,
            Location = new Point(260, 201),
            Size = new Size(80, 28)
        };

        Controls.AddRange([title, version, creator, github, website, updates, close]);
        AcceptButton = close;
        CancelButton = close;
    }

    private static LinkLabel Link(string text, string url, Point location)
    {
        var link = new LinkLabel { Text = text, AutoSize = true, Location = location };
        link.LinkClicked += (_, _) => Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        return link;
    }
}
