namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnHeroCard : RnCard
{
    public RnHeroCard(string name, string details, bool canActivate, Func<Task>? activate, Action capture)
    {
        var heroAccent = RnTheme.Purple;
        Dock = DockStyle.Fill;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = Color.FromArgb(23, 28, 42);
        BorderColor = Color.FromArgb(112, RnTheme.Purple);
        Padding = new Padding(LayoutTokens.HeroInset);

        var hero = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        hero.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        hero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        hero.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        hero.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var iconContainer = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 24, 0),
            Anchor = AnchorStyles.None
        };
        iconContainer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        iconContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        iconContainer.Controls.Add(new RnIconBadge(heroAccent, "✦")
        {
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = Padding.Empty
        }, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 24, 0)
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        content.Controls.Add(new Label
        {
            Text = "✦  ACTIVE ENVIRONMENT",
            ForeColor = heroAccent,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6),
            Anchor = AnchorStyles.Left
        }, 0, 0);

        content.Controls.Add(new RnWrappingLabel
        {
            Text = name,
            ForeColor = RnTheme.PrimaryText,
            Font = new Font("Segoe UI Semibold", 22),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 6)
        }, 0, 1);

        content.Controls.Add(new RnWrappingLabel
        {
            Text = details,
            ForeColor = RnTheme.SecondaryText,
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10)
        }, 0, 2);

        content.Controls.Add(new Label
        {
            Text = "●  Ready",
            ForeColor = RnTheme.Success,
            AutoSize = true,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left
        }, 0, 3);

        var actions = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 8, 0),
            Anchor = AnchorStyles.Right
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var primary = new RnButton(canActivate ? Color.FromArgb(155, 109, 255) : Color.FromArgb(38, 198, 190))
        {
            Text = canActivate ? "Activate" : "Capture Current Setup",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10)
        };
        if (canActivate && activate is not null)
            primary.Click += async (_, _) => await activate();
        else
            primary.Click += (_, _) => capture();

        var manage = new RnButton(Color.FromArgb(44, 54, 60))
        {
            Text = "Manage",
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };

        actions.Controls.Add(primary, 0, 0);
        actions.Controls.Add(manage, 0, 1);

        hero.Controls.Add(iconContainer, 0, 0);
        hero.Controls.Add(content, 1, 0);
        hero.Controls.Add(actions, 2, 0);
        Controls.Add(hero);
    }
}

internal sealed class RnWrappingLabel : Label
{
    public override Size GetPreferredSize(Size proposedSize)
    {
        var availableWidth = proposedSize.Width > 0 ? proposedSize.Width : Parent?.ClientSize.Width ?? 0;
        if (availableWidth <= 0)
            return base.GetPreferredSize(proposedSize);

        var measured = TextRenderer.MeasureText(
            Text,
            Font,
            new Size(availableWidth, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
        return new Size(availableWidth, measured.Height);
    }
}

internal sealed class RnIconBadge : Control
{
    private readonly Color _accent;
    private readonly string _symbol;

    public RnIconBadge(Color accent, string symbol)
    {
        _accent = accent;
        _symbol = symbol;
        SetStyle(
            ControlStyles.SupportsTransparentBackColor |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw,
            true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var side = Font.Height * 6;
        return new Size(side, side);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SetClip(ClientRectangle);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var inset = Math.Max(2, DeviceDpi / 24);
        var badgeBounds = Rectangle.Inflate(ClientRectangle, -inset, -inset);
        if (badgeBounds.Width <= 0 || badgeBounds.Height <= 0)
            return;

        using var background = new SolidBrush(Color.FromArgb(45, _accent.R, _accent.G, _accent.B));
        using var border = new Pen(_accent, Math.Max(1f, DeviceDpi / 48f));
        using var symbolFont = new Font("Segoe UI", Font.SizeInPoints * 3);
        e.Graphics.FillEllipse(background, badgeBounds);
        e.Graphics.DrawEllipse(border, badgeBounds);
        TextRenderer.DrawText(
            e.Graphics,
            _symbol,
            symbolFont,
            badgeBounds,
            Color.White,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoPadding |
            TextFormatFlags.NoClipping);
    }
}
