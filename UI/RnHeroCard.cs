namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnHeroCard : RnCard
{
    public RnHeroCard(string name, string details, bool canActivate, Func<Task>? activate, Action capture)
    {
        Dock = DockStyle.Fill; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink; Padding = new Padding(LayoutTokens.HeroInset);
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, RowCount = 1, BackColor = Color.Transparent };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var icon = new RnIconBadge(Color.FromArgb(38,198,190), "✦") { Anchor = AnchorStyles.None, Margin = new Padding(0, 0, 16, 0) }; grid.Controls.Add(icon, 0, 0);
        var text = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent }; text.Controls.Add(new Label { Text = "✦  ACTIVE ENVIRONMENT", ForeColor = Color.FromArgb(38,198,190), AutoSize = true }, 0, 0); text.Controls.Add(new Label { Text = name, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 22), AutoSize = true, MaximumSize = new Size(520, 0) }, 0, 1); text.Controls.Add(new Label { Text = details, ForeColor = Color.FromArgb(174,187,194), AutoSize = true, MaximumSize = new Size(520, 0), Padding = new Padding(0, 4, 0, 4) }, 0, 2); text.Controls.Add(new Label { Text = "●  Ready", ForeColor = Color.LightGreen, AutoSize = true }, 0, 3); grid.Controls.Add(text, 1, 0);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent, Margin = new Padding(16, 8, 0, 0) }; var primary = new RnButton(canActivate ? Color.FromArgb(155,109,255) : Color.FromArgb(38,198,190)) { Text = canActivate ? "Activate" : "Capture Current Setup", AutoSize = true, MinimumSize = new Size(150, LayoutTokens.ButtonHeight) }; if (canActivate && activate is not null) primary.Click += async (_, _) => await activate(); else primary.Click += (_, _) => capture(); var manage = new RnButton(Color.FromArgb(44,54,60)) { Text = "Manage", AutoSize = true, MinimumSize = new Size(150, LayoutTokens.ButtonHeight) }; actions.Controls.AddRange([primary, manage]); grid.Controls.Add(actions, 2, 0); Controls.Add(grid);
    }
}
internal sealed class RnIconBadge : Control
{
    private readonly Color _accent; private readonly string _symbol;
    public RnIconBadge(Color accent, string symbol) { _accent = accent; _symbol = symbol; Size = new Size(88, 88); BackColor = Color.Transparent; DoubleBuffered = true; }
    protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; using var b = new SolidBrush(Color.FromArgb(45, _accent.R, _accent.G, _accent.B)); e.Graphics.FillEllipse(b, 4, 4, Width - 8, Height - 8); using var p = new Pen(_accent, 2); e.Graphics.DrawEllipse(p, 4, 4, Width - 8, Height - 8); TextRenderer.DrawText(e.Graphics, _symbol, new Font("Segoe UI", 30), ClientRectangle, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
}
