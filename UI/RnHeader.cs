namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnHeader : RnCard
{
    private const int PreferredHeaderHeight = 124;
    public RnHeader(string version, EventHandler settings, EventHandler update)
    {
        Dock = DockStyle.Fill; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowOnly; MinimumSize = new Size(0, PreferredHeaderHeight); Padding = new Padding(LayoutTokens.HeaderPadding, LayoutTokens.HeaderPadding, LayoutTokens.HeaderPadding, LayoutTokens.HeaderPadding + 16);
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 3, RowCount = 2, BackColor = Color.Transparent };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize)); grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var mark = new RnStarMark { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 0) }; grid.Controls.Add(mark, 0, 0); grid.SetRowSpan(mark, 2);
        grid.Controls.Add(new Label { Text = "RENDERNORTH", ForeColor = Color.FromArgb(38,198,190), Font = new Font("Segoe UI Semibold", 9), AutoSize = true }, 1, 0);
        grid.Controls.Add(new Label { Text = "Environments", ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 22), AutoSize = true, Padding = new Padding(0, 0, 0, 10), Margin = new Padding(0, 0, 0, 8) }, 1, 1);
        var metadata = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Margin = new Padding(12, 22, 8, 0) }; metadata.Controls.Add(new Label { Text = "Your PC should adapt to what you're doing.", ForeColor = Color.FromArgb(174,187,194), AutoSize = true }); metadata.Controls.Add(new Label { Text = " • v" + version, ForeColor = Color.FromArgb(38,198,190), AutoSize = true }); grid.Controls.Add(metadata, 2, 0);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, BackColor = Color.Transparent, Margin = new Padding(4, 4, 0, 0) }; var updateButton = new RnButton(Color.FromArgb(44,54,60)) { Text = "Check for Updates", AutoSize = true, MinimumSize = new Size(126, LayoutTokens.ButtonHeight) }; updateButton.Click += update; var settingsButton = new RnButton(Color.FromArgb(44,54,60)) { Text = "Settings", AutoSize = true, MinimumSize = new Size(92, LayoutTokens.ButtonHeight) }; settingsButton.Click += settings; actions.Controls.AddRange([updateButton, settingsButton]); grid.Controls.Add(actions, 2, 1); Controls.Add(grid);
    }

    public override Size GetPreferredSize(Size proposedSize) => new Size(proposedSize.Width, PreferredHeaderHeight);
}
internal sealed class RnStarMark : Control
{
    public RnStarMark() { DoubleBuffered = true; BackColor = Color.Transparent; MinimumSize = new Size(42, 42); }
    protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; var c = new PointF(Width / 2f, Height / 2f); using var pen = new Pen(Color.FromArgb(38,198,190), 2); e.Graphics.DrawLine(pen, c.X, 4, c.X, Height - 4); e.Graphics.DrawLine(pen, 4, c.Y, Width - 4, c.Y); e.Graphics.DrawEllipse(pen, c.X - 10, c.Y - 10, 20, 20); }
}
