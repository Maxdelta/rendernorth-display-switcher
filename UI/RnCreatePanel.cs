namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnCreatePanel : RnCard
{
    public RnCreatePanel(EventHandler newEnv, EventHandler capture, EventHandler identify, EventHandler settings)
    { Dock = DockStyle.Top; AutoSize = true; Padding = new Padding(LayoutTokens.CardInset); BorderColor = Color.FromArgb(72, RnTheme.Accent); var root = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent }; root.Controls.Add(new Label { Text = "⚡  QUICK ACTIONS", ForeColor = RnTheme.Accent, Font = new Font("Segoe UI Semibold", 9), AutoSize = true },0,0); var row = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent }; row.Controls.AddRange([new RnActionTile("＋  New Environment",newEnv),new RnActionTile("▣  Capture Current Setup",capture),new RnActionTile("▤  Identify Displays",identify),new RnActionTile("⚙  Settings",settings)]); root.Controls.Add(row,0,1); Controls.Add(root); }
}
