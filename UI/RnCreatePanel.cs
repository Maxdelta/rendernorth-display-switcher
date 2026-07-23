namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnCreatePanel : RnCard
{
    private const int MaximumContentWidth = 760;
    private const int CollapseBelowWidth = 660;
    private const int ExpandAboveWidth = 700;
    private readonly TableLayoutPanel _content;
    private readonly FlowLayoutPanel _actions;
    private bool _compactActions;

    public RnCreatePanel(EventHandler newEnv, EventHandler capture, EventHandler identify, EventHandler settings)
    {
        Dock = DockStyle.Top;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(LayoutTokens.CardInset, 8, LayoutTokens.CardInset, 8);
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
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        center.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _content = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left
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
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 1, 3, 2)
        }, 0, 0);

        _actions = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Anchor = AnchorStyles.Left
        };

        var actions = new[]
        {
            new RnActionTile("＋  New Environment", newEnv) { Dock = DockStyle.Fill, Margin = new Padding(4, 2, 4, 2) },
            new RnActionTile("▣  Capture Current Setup", capture) { Dock = DockStyle.Fill, Margin = new Padding(4, 2, 4, 2) },
            new RnActionTile("▤  Identify Displays", identify) { Dock = DockStyle.Fill, Margin = new Padding(4, 2, 4, 2) },
            new RnActionTile("⚙  Settings", settings) { Dock = DockStyle.Fill, Margin = new Padding(4, 2, 4, 2) }
        };
        var preferredWidths = new[] { 170, 210, 170, 130 };
        for (var index = 0; index < actions.Length; index++)
        {
            actions[index].Dock = DockStyle.None;
            actions[index].Size = new Size(preferredWidths[index], actions[index].Height);
            actions[index].MinimumSize = actions[index].Size;
            _actions.Controls.Add(actions[index]);
        }

        _content.Controls.Add(_actions, 0, 1);
        center.Controls.Add(_content, 1, 0);
        Controls.Add(center);
    }

    internal Control ContentHost => _content;
    internal int ActionColumnCount => _compactActions ? 2 : 4;

    internal void ApplyAvailableWidth(int availableWidth)
    {
        if (availableWidth <= 0)
            return;

        var outerWidth = Math.Max(1, availableWidth - Margin.Horizontal);
        var contentWidth = Math.Min(MaximumContentWidth, Math.Max(1, outerWidth - Padding.Horizontal));
        var compact = _compactActions ? contentWidth < ExpandAboveWidth : contentWidth < CollapseBelowWidth;
        var widthChanged = MinimumSize.Width != outerWidth || MaximumSize.Width != outerWidth ||
            _content.MinimumSize.Width != contentWidth || _content.MaximumSize.Width != contentWidth;
        if (!widthChanged && compact == _compactActions)
            return;

        SuspendLayout();
        _content.SuspendLayout();
        _actions.SuspendLayout();
        if (widthChanged)
        {
            MinimumSize = new Size(outerWidth, 0);
            MaximumSize = new Size(outerWidth, 0);
            _content.MinimumSize = new Size(contentWidth, 0);
            _content.MaximumSize = new Size(contentWidth, 0);
        }
        if (compact != _compactActions || widthChanged)
            ConfigureActionFlow(compact, contentWidth);
        _actions.ResumeLayout(false);
        _content.ResumeLayout(false);
        ResumeLayout(true);
    }

    private void ConfigureActionFlow(bool compact, int availableWidth)
    {
        _compactActions = compact;
        _actions.WrapContents = compact;
        _actions.MaximumSize = compact ? new Size(Math.Min(availableWidth, 396), 0) : Size.Empty;
    }
}
