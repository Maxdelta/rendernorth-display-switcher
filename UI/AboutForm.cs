using System.Diagnostics;
using RenderNorth.DisplaySwitcher.Services;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class AboutForm : Form
{
    private const string GitHubUrl = UpdateService.GitHubRepositoryUrl;
    private const string WebsiteUrl = "https://www.rendernorth.com/";

    public AboutForm(Func<Task> checkForUpdates)
    {
        Text = "About RenderNorth Display Switcher";
        ClientSize = new Size(370, 275);
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
        var version = new Label { Text = $"Version {AppVersion.Current}", AutoSize = true, Location = new Point(31, 65) };
        var creator = new Label { Text = "Created by RenderNorth", AutoSize = true, Location = new Point(31, 92) };
        var github = Link("GitHub", GitHubUrl, new Point(31, 128));
        var website = Link("Website", WebsiteUrl, new Point(105, 128));
        var license = Link("MIT License", GitHubUrl + "/blob/main/LICENSE", new Point(175, 128));
        var copyright = new Label { Text = "Copyright 2026 RenderNorth", AutoSize = true, Location = new Point(31, 154) };
        var updates = new Button
        {
            Text = "Check for Updates",
            Location = new Point(31, 181),
            Size = new Size(205, 32)
        };
        updates.Click += async (_, _) => { updates.Enabled = false; await checkForUpdates(); updates.Enabled = true; };
        var close = new Button
        {
            Text = "Close",
            DialogResult = DialogResult.OK,
            Location = new Point(260, 235),
            Size = new Size(80, 28)
        };

        Controls.AddRange([title, version, creator, github, website, license, copyright, updates, close]);
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
