namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnCreatePanel : RnCard
{
    private const int MaximumContentWidth = 760;
    private const int CollapseBelowWidth = 660;
    private const int ExpandAboveWidth = 700;
    private readonly TableLayoutPanel _content;
    private readonly TableLayoutPanel _actions;
    private bool _compactActions;

    public RnCreatePanel(EventHandler newEnv, EventHandler capture, EventHandler identify, EventHandler settings)
    {
        Dock = DockStyle.Top;
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

        _actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent,
            Margin = Padding.Empty
        };
        ConfigureActionGrid(compact: false);

        var actions = new[]
        {
            new RnActionTile("＋  New Environment", newEnv) { Dock = DockStyle.Fill, Margin = new Padding(4) },
            new RnActionTile("▣  Capture Current Setup", capture) { Dock = DockStyle.Fill, Margin = new Padding(4) },
            new RnActionTile("▤  Identify Displays", identify) { Dock = DockStyle.Fill, Margin = new Padding(4) },
            new RnActionTile("⚙  Settings", settings) { Dock = DockStyle.Fill, Margin = new Padding(4) }
        };
        for (var index = 0; index < actions.Length; index++)
            _actions.Controls.Add(actions[index], index, 0);

        _content.Controls.Add(_actions, 0, 1);
        center.Controls.Add(_content, 1, 0);
        Controls.Add(center);
    }

    internal Control ContentHost => _content;
    internal int ActionColumnCount => _actions.ColumnCount;

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
        if (compact != _compactActions)
            ConfigureActionGrid(compact);
        _actions.ResumeLayout(false);
        _content.ResumeLayout(false);
        ResumeLayout(true);
    }

    private void ConfigureActionGrid(bool compact)
    {
        _compactActions = compact;
        _actions.ColumnStyles.Clear();
        _actions.RowStyles.Clear();
        if (compact)
        {
            _actions.RowCount = 2;
            for (var index = 0; index < _actions.Controls.Count; index++)
                _actions.SetCellPosition(_actions.Controls[index], new TableLayoutPanelCellPosition(index % 2, index / 2));
            _actions.ColumnCount = 2;
        }
        else
        {
            _actions.ColumnCount = 4;
            _actions.RowCount = 1;
            for (var index = 0; index < _actions.Controls.Count; index++)
                _actions.SetCellPosition(_actions.Controls[index], new TableLayoutPanelCellPosition(index, 0));
        }
        for (var column = 0; column < _actions.ColumnCount; column++)
            _actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / _actions.ColumnCount));
        for (var row = 0; row < _actions.RowCount; row++)
            _actions.RowStyles.Add(new RowStyle(SizeType.AutoSize));
    }
}
