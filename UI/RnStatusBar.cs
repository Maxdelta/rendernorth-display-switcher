namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnStatusBar : RnCard
{
    public RnStatusBar(string result, string success, string update) { Dock = DockStyle.Top; AutoSize = true; Padding = new Padding(LayoutTokens.StatusInset); var row = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Color.Transparent }; row.Controls.AddRange([new Label { Text = "●  System Ready", ForeColor = RnTheme.Success, AutoSize = true, Margin = new Padding(4) },new Label { Text = "│  " + result, ForeColor = RnTheme.PrimaryText, AutoSize = true, Margin = new Padding(4) },new Label { Text = "│  " + update, ForeColor = RnTheme.SecondaryText, AutoSize = true, Margin = new Padding(4) }]); Controls.Add(row); }
}
