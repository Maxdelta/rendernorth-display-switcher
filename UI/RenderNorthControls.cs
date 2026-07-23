using System.Drawing.Drawing2D;

namespace RenderNorth.DisplaySwitcher.UI;

internal static class RnTheme
{
    public static readonly Color Background = Color.FromArgb(15, 20, 26);
    public static readonly Color Card = Color.FromArgb(21, 28, 36);
    public static readonly Color Control = Color.FromArgb(27, 37, 48);
    public static readonly Color Border = Color.FromArgb(36, 49, 61);
    public static readonly Color PrimaryText = Color.FromArgb(230, 237, 243);
    public static readonly Color SecondaryText = Color.FromArgb(155, 167, 180);
    public static readonly Color Accent = Color.FromArgb(0, 212, 196);
    public static readonly Color Purple = Color.FromArgb(157, 78, 221);
    public static readonly Color Success = Color.FromArgb(52, 199, 89);

    public static Color OpaqueAncestorBackColor(Control control)
    {
        for (var parent = control.Parent; parent is not null; parent = parent.Parent)
            if (parent.BackColor.A == byte.MaxValue)
                return parent.BackColor;
        return Background;
    }
}

internal class RnCard : Panel
{
    public int Radius { get; set; } = 14;
    public Color BorderColor { get; set; } = RnTheme.Border;
    public bool Hovered { get; private set; }

    public RnCard()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        BackColor = RnTheme.Card;
        ForeColor = RnTheme.PrimaryText;
        MouseEnter += (_, _) => { Hovered = true; Invalidate(); };
        MouseLeave += (_, _) => { Hovered = false; Invalidate(); };
        Padding = new Padding(8);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        // Painting is decorative only. Do not assign a Region: child controls own
        // their logical layout and must never be clipped by the rounded border.
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.SetClip(ClientRectangle);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(RnTheme.OpaqueAncestorBackColor(this));
        var shadowBounds = new Rectangle(2, 3, Math.Max(1, ClientSize.Width - 5), Math.Max(1, ClientSize.Height - 6));
        var surfaceBounds = new Rectangle(1, 1, Math.Max(1, ClientSize.Width - 3), Math.Max(1, ClientSize.Height - 4));
        var radius = Math.Min(Radius, Math.Max(2, Math.Min(ClientSize.Width, ClientSize.Height) / 4));
        using var shadowPath = RoundedPath(shadowBounds, radius);
        using var surfacePath = RoundedPath(surfaceBounds, radius);
        using var shadow = new SolidBrush(Color.FromArgb(88, 8, 13, 18));
        using var surface = new SolidBrush(BackColor);
        using var border = new Pen(Hovered ? Color.FromArgb(56, 88, 96) : BorderColor);
        e.Graphics.FillPath(shadow, shadowPath);
        e.Graphics.FillPath(surface, surfacePath);
        e.Graphics.DrawPath(border, surfacePath);
    }

    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); }

    internal static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath(); var d = radius * 2; var r = new Rectangle(bounds.X, bounds.Y, Math.Max(1, bounds.Width - 1), Math.Max(1, bounds.Height - 1));
        path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90); path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path;
    }
}

internal class RnButton : Button
{
    private readonly bool _primary;
    private readonly Color _baseColor;
    private Color _accentColor = Color.Empty;

    public Color AccentColor
    {
        get => _accentColor;
        set { _accentColor = value; Invalidate(); }
    }

    public RnButton(Color color)
    {
        _primary = Math.Max(color.R, Math.Max(color.G, color.B)) - Math.Min(color.R, Math.Min(color.G, color.B)) > 32;
        var usesPurpleAccent = color.B > color.G && color.R > color.G;
        _baseColor = _primary ? (usesPurpleAccent ? RnTheme.Purple : RnTheme.Accent) : RnTheme.Control;
        BackColor = _baseColor;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = _baseColor;
        FlatAppearance.MouseDownBackColor = _baseColor;
        ForeColor = RnTheme.PrimaryText;
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        UseVisualStyleBackColor = false;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
    }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); BackColor = _primary ? ControlPaint.Light(_baseColor, .08f) : Color.FromArgb(34, 47, 60); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); BackColor = _baseColor; }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); BackColor = _primary ? ControlPaint.Dark(_baseColor, .10f) : Color.FromArgb(22, 31, 40); }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); BackColor = ClientRectangle.Contains(PointToClient(Cursor.Position)) ? (_primary ? ControlPaint.Light(_baseColor, .08f) : Color.FromArgb(34, 47, 60)) : _baseColor; }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SetClip(ClientRectangle);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(RnTheme.OpaqueAncestorBackColor(this));
        var bounds = new Rectangle(1, 1, Math.Max(1, ClientSize.Width - 3), Math.Max(1, ClientSize.Height - 3));
        using var path = RnCard.RoundedPath(bounds, 8);
        using var fill = new SolidBrush(BackColor);
        var outline = !_primary && !_accentColor.IsEmpty ? Color.FromArgb(112, _accentColor) : (_primary ? Color.FromArgb(76, _baseColor) : RnTheme.Border);
        using var border = new Pen(outline);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
        if (Focused && ShowFocusCues)
        {
            var focusBounds = Rectangle.Inflate(bounds, -3, -3);
            using var focusPath = RnCard.RoundedPath(focusBounds, 6);
            using var focusPen = new Pen(_primary ? RnTheme.PrimaryText : (_accentColor.IsEmpty ? RnTheme.Accent : _accentColor))
            {
                DashStyle = DashStyle.Dot
            };
            e.Graphics.DrawPath(focusPen, focusPath);
        }

        var parts = Text.Split(["  "], 2, StringSplitOptions.None);
        if (!_primary && !_accentColor.IsEmpty && parts.Length == 2)
        {
            var scale = DeviceDpi / 96f;
            var iconWidth = Math.Max(24, (int)Math.Round(30 * scale));
            var inset = Math.Max(4, (int)Math.Round(6 * scale));
            var iconBounds = new Rectangle(bounds.Left + inset, bounds.Top, iconWidth, bounds.Height);
            var labelBounds = new Rectangle(iconBounds.Right, bounds.Top, Math.Max(1, bounds.Right - iconBounds.Right - inset), bounds.Height);
            using var iconWash = new SolidBrush(Color.FromArgb(28, _accentColor));
            e.Graphics.FillEllipse(iconWash, Rectangle.Inflate(iconBounds, -2, -5));
            TextRenderer.DrawText(e.Graphics, parts[0], Font, iconBounds, _accentColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(e.Graphics, parts[1], Font, labelBounds, ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
        else
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, bounds, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}

internal sealed class RnDisplayPreview : Control
{
    public string Category { get; set; } = "Custom";
    public RnDisplayPreview() { DoubleBuffered = true; Size = new Size(132, 42); BackColor = Color.Transparent; }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; var accent = Category.ToLowerInvariant() switch { "gaming" => Color.FromArgb(155,109,255), "streaming" => Color.FromArgb(240,93,103), "development" => Color.FromArgb(76,154,255), "presentation" => Color.FromArgb(76,203,138), _ => Color.FromArgb(38,198,190) };
        var count = Category.Equals("Presentation", StringComparison.OrdinalIgnoreCase) ? 2 : 3; for (var i = 0; i < count; i++) { var x = 4 + i * 42; using var pen = new Pen(i == 0 ? accent : Color.FromArgb(120, accent), 2); e.Graphics.DrawRectangle(pen, x, 5, 30, 20); e.Graphics.DrawLine(pen, x + 10, 29, x + 20, 29); e.Graphics.DrawLine(pen, x + 15, 25, x + 15, 29); }
        using var textBrush = new SolidBrush(Color.FromArgb(180, accent)); e.Graphics.DrawString(count + " displays", Font, textBrush, 4, 30);
    }
}
