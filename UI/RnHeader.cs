namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnHeader : RnCard
{
    public RnHeader(string version, EventHandler settings, EventHandler update)
    {
        Dock = DockStyle.Fill;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(
            LayoutTokens.HeaderPadding,
            LayoutTokens.HeaderPadding,
            LayoutTokens.HeaderPadding,
            LayoutTokens.HeaderPadding + 16);

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var brand = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left
        };
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var mark = new RnStarMark
        {
            AutoSize = true,
            Margin = new Padding(0, 4, 10, 0),
            Anchor = AnchorStyles.None
        };
        brand.Controls.Add(mark, 0, 0);
        brand.SetRowSpan(mark, 2);

        brand.Controls.Add(new Label
        {
            Text = "RENDERNORTH",
            ForeColor = Color.FromArgb(38, 198, 190),
            Font = new Font("Segoe UI Semibold", 9),
            AutoSize = true,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left
        }, 1, 0);

        brand.Controls.Add(new Label
        {
            Text = "Environments",
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 22),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12),
            Anchor = AnchorStyles.Left
        }, 1, 1);

        var metadata = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(16, 0, 0, 0),
            Anchor = AnchorStyles.Right
        };
        metadata.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        metadata.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        metadata.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        metadata.Controls.Add(new Label
        {
            Text = "Your PC should adapt to what you're doing.",
            ForeColor = Color.FromArgb(174, 187, 194),
            AutoSize = true,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.None
        }, 0, 0);
        metadata.Controls.Add(new Label
        {
            Text = " • v" + version,
            ForeColor = Color.FromArgb(38, 198, 190),
            AutoSize = true,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.None
        }, 1, 0);

        var actions = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = new Padding(16, 0, 0, 12),
            Anchor = AnchorStyles.Right
        };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var updateButton = new RnButton(Color.FromArgb(44, 54, 60))
        {
            Text = "Check for Updates",
            AutoSize = true,
            Margin = new Padding(0, 0, 8, 0),
            Anchor = AnchorStyles.None
        };
        updateButton.Click += update;

        var settingsButton = new RnButton(Color.FromArgb(44, 54, 60))
        {
            Text = "Settings",
            AutoSize = true,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.None
        };
        settingsButton.Click += settings;

        actions.Controls.Add(updateButton, 0, 0);
        actions.Controls.Add(settingsButton, 1, 0);

        header.Controls.Add(brand, 0, 0);
        header.SetRowSpan(brand, 2);
        header.Controls.Add(metadata, 1, 0);
        header.Controls.Add(actions, 1, 1);
        Controls.Add(header);
    }
}

internal sealed class RnStarMark : Control
{
    public RnStarMark()
    {
        SetStyle(
            ControlStyles.SupportsTransparentBackColor |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw,
            true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var side = Font.Height * 3;
        return new Size(side, side);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var center = new PointF(ClientRectangle.Left + ClientRectangle.Width / 2f, ClientRectangle.Top + ClientRectangle.Height / 2f);
        var inset = Math.Max(2f, DeviceDpi / 24f);
        var orbitRadius = Math.Min(ClientRectangle.Width, ClientRectangle.Height) / 4f;
        using var pen = new Pen(Color.FromArgb(38, 198, 190), Math.Max(1f, DeviceDpi / 48f));
        e.Graphics.DrawLine(pen, center.X, ClientRectangle.Top + inset, center.X, ClientRectangle.Bottom - inset);
        e.Graphics.DrawLine(pen, ClientRectangle.Left + inset, center.Y, ClientRectangle.Right - inset, center.Y);
        e.Graphics.DrawEllipse(
            pen,
            center.X - orbitRadius,
            center.Y - orbitRadius,
            orbitRadius * 2,
            orbitRadius * 2);
    }
}
