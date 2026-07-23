namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnEmptyState : RnCard
{
    private const int MaximumContentWidth = 760;
    private readonly TableLayoutPanel _content;

    public RnEmptyState(Action capture, Action create)
    {
        Dock = DockStyle.Top;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BorderColor = Color.FromArgb(64, RnTheme.Accent);
        Padding = new Padding(LayoutTokens.HeroInset);

        var host = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        host.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var center = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        center.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _content = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.None
        };
        _content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < _content.RowCount; row++)
            _content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var mark = new RnStarMark
        {
            Anchor = AnchorStyles.None,
            Size = new Size(52, 52),
            Margin = new Padding(3, 0, 3, 2)
        };
        _content.Controls.Add(mark, 0, 0);

        _content.Controls.Add(new Label
        {
            Text = "Welcome to RenderNorth Environments",
            ForeColor = RnTheme.PrimaryText,
            Font = new Font("Segoe UI Semibold", 18),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(3, 2, 3, 7)
        }, 0, 1);

        _content.Controls.Add(new Label
        {
            Text = "Your PC should adapt to what you're doing.",
            ForeColor = RnTheme.SecondaryText,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None,
            Margin = new Padding(3, 0, 3, 2)
        }, 0, 2);

        _content.Controls.Add(new Label
        {
            Text = "Create your first environment by saving your current display setup.",
            ForeColor = RnTheme.SecondaryText,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None,
            Margin = new Padding(3, 0, 3, 10)
        }, 0, 3);

        var tiles = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoScroll = false,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.None,
            Padding = new Padding(4),
            Margin = new Padding(3, 0, 3, 1)
        };
        foreach (var item in new[]
        {
            ("◇", "Gaming", RnTheme.Purple),
            ("⌘", "Development", Color.FromArgb(58, 161, 255)),
            ("◉", "Streaming", Color.FromArgb(255, 69, 83)),
            ("▣", "Presentation", RnTheme.Success),
            ("✈", "Travel", RnTheme.Accent)
        })
        {
            var tile = new RnButton(RnTheme.Control)
            {
                Text = item.Item1 + "  " + item.Item2,
                AccentColor = item.Item3,
                Size = new Size(132, LayoutTokens.ButtonHeight),
                Margin = new Padding(4)
            };
            tile.Click += (_, _) => capture();
            tiles.Controls.Add(tile);
        }
        _content.Controls.Add(tiles, 0, 4);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Anchor = AnchorStyles.None,
            Margin = Padding.Empty
        };

        var captureButton = new RnButton(Color.FromArgb(38, 198, 190))
        {
            Text = "Capture Current Setup",
            AutoSize = true,
            MinimumSize = new Size(180, LayoutTokens.ButtonHeight)
        };
        captureButton.Click += (_, _) => capture();

        var newEnvironmentButton = new RnButton(Color.FromArgb(53, 64, 70))
        {
            Text = "New Environment",
            AutoSize = true,
            MinimumSize = new Size(150, LayoutTokens.ButtonHeight)
        };
        newEnvironmentButton.Click += (_, _) => create();

        actions.Controls.AddRange([captureButton, newEnvironmentButton]);
        _content.Controls.Add(actions, 0, 5);

        center.Controls.Add(_content, 1, 0);
        host.Controls.Add(center, 0, 1);
        Controls.Add(host);
    }

    internal Control ContentHost => _content;

    internal void ApplyAvailableWidth(int availableWidth)
    {
        if (availableWidth <= 0)
            return;

        var outerWidth = Math.Max(1, availableWidth - Margin.Horizontal);
        var contentWidth = Math.Min(MaximumContentWidth, Math.Max(1, outerWidth - Padding.Horizontal));
        if (MinimumSize.Width == outerWidth && MaximumSize.Width == outerWidth && _content.MaximumSize.Width == contentWidth)
            return;

        MinimumSize = new Size(outerWidth, 0);
        MaximumSize = new Size(outerWidth, 0);
        _content.MaximumSize = new Size(contentWidth, 0);
    }
}
