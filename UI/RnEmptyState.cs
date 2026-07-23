namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnEmptyState : RnCard
{
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

        var content = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.None
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        for (var row = 0; row < content.RowCount; row++)
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var mark = new RnStarMark
        {
            Anchor = AnchorStyles.None,
            Size = new Size(52, 52),
            Margin = new Padding(3, 0, 3, 2)
        };
        content.Controls.Add(mark, 0, 0);

        content.Controls.Add(new Label
        {
            Text = "Welcome to RenderNorth Environments",
            ForeColor = RnTheme.PrimaryText,
            Font = new Font("Segoe UI Semibold", 18),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(3, 2, 3, 7)
        }, 0, 1);

        content.Controls.Add(new Label
        {
            Text = "Your PC should adapt to what you're doing.",
            ForeColor = RnTheme.SecondaryText,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Anchor = AnchorStyles.None,
            Margin = new Padding(3, 0, 3, 2)
        }, 0, 2);

        content.Controls.Add(new Label
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
        content.Controls.Add(tiles, 0, 4);

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
        content.Controls.Add(actions, 0, 5);

        host.Controls.Add(content, 0, 1);
        Controls.Add(host);
    }
}
