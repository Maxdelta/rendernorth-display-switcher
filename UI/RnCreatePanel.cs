namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnCreatePanel : RnCard
{
    private const int MaximumContentWidth = 760;
    private readonly TableLayoutPanel _content;

    public RnCreatePanel(EventHandler newEnv, EventHandler capture, EventHandler identify, EventHandler settings)
    {
        Dock = DockStyle.Top;
        Anchor = AnchorStyles.Left | AnchorStyles.Right;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(LayoutTokens.CardInset);
        BorderColor = Color.FromArgb(72, RnTheme.Accent);

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
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.None
        };
        _content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _content.Controls.Add(new Label
        {
            Text = "⚡  QUICK ACTIONS",
            ForeColor = RnTheme.Accent,
            Font = new Font("Segoe UI Semibold", 9),
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        actions.Controls.AddRange(
        [
            new RnActionTile("＋  New Environment", newEnv),
            new RnActionTile("▣  Capture Current Setup", capture),
            new RnActionTile("▤  Identify Displays", identify),
            new RnActionTile("⚙  Settings", settings)
        ]);
        _content.Controls.Add(actions, 0, 1);

        center.Controls.Add(_content, 1, 0);
        Controls.Add(center);
    }

    internal Control ContentHost => _content;

    internal void ApplyAvailableWidth(int availableWidth)
    {
        if (availableWidth <= 0)
            return;

        var outerWidth = Math.Max(1, availableWidth - Margin.Horizontal);
        var contentWidth = Math.Min(MaximumContentWidth, Math.Max(1, outerWidth - Padding.Horizontal));
        MinimumSize = new Size(outerWidth, 0);
        MaximumSize = new Size(outerWidth, 0);
        _content.MaximumSize = new Size(contentWidth, 0);
        PerformLayout();
    }
}
