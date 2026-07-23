namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnStatusBar : RnCard
{
    public RnStatusBar(string result, string success, string update) { Dock = DockStyle.Top; AutoSize = true; Padding = new Padding(LayoutTokens.StatusInset); var row = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Color.Transparent }; row.Controls.AddRange([new Label { Text = "●  System Ready", ForeColor = Color.LightGreen, AutoSize = true, Margin = new Padding(4) },new Label { Text = "│  " + result, ForeColor = Color.White, AutoSize = true, Margin = new Padding(4) },new Label { Text = "│  " + update, ForeColor = Color.FromArgb(174,187,194), AutoSize = true, Margin = new Padding(4) }]); Controls.Add(row); }
}
