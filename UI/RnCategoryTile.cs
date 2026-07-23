namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnCategoryTile : RnCard
{
    public RnCategoryTile(string name, Color accent, Action click)
    { AutoSize = true; Size = new Size(132, 78); MinimumSize = new Size(112, 72); Padding = new Padding(8); Cursor = Cursors.Hand; BorderColor = Color.FromArgb(96, accent); var box = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 2, BackColor = Color.Transparent }; box.Controls.Add(new Label { Text = "◆", ForeColor = accent, Font = new Font("Segoe UI", 18), Anchor = AnchorStyles.None, AutoSize = true }, 0, 0); box.Controls.Add(new Label { Text = name, ForeColor = RnTheme.PrimaryText, Font = new Font("Segoe UI Semibold", 9), Anchor = AnchorStyles.None, AutoSize = true }, 0, 1); Controls.Add(box); Click += (_,_) => click(); foreach(Control child in box.Controls) child.Click += (_,_) => click(); }
}
