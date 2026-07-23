using System.Drawing.Drawing2D;

namespace RenderNorth.DisplaySwitcher.UI;

internal class RnCard : Panel
{
    public int Radius { get; set; } = 12;
    public Color BorderColor { get; set; } = Color.FromArgb(58, 70, 77);
    public bool Hovered { get; private set; }

    public RnCard()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
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
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var safe = new Rectangle(1, 1, Math.Max(1, ClientSize.Width - 3), Math.Max(1, ClientSize.Height - 3));
        using var path = RoundedPath(safe, Math.Min(Radius, Math.Max(2, Math.Min(Width, Height) / 4)));
        using var brush = new SolidBrush(BackColor);
        using var pen = new Pen(Hovered ? Color.FromArgb(90, BorderColor) : BorderColor, 1);
        e.Graphics.FillPath(brush, path); e.Graphics.DrawPath(pen, path);
    }

    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); }

    internal static GraphicsPath RoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath(); var d = radius * 2; var r = new Rectangle(bounds.X, bounds.Y, Math.Max(1, bounds.Width - 1), Math.Max(1, bounds.Height - 1));
        path.AddArc(r.X, r.Y, d, d, 180, 90); path.AddArc(r.Right - d, r.Y, d, d, 270, 90); path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); path.AddArc(r.X, r.Bottom - d, d, d, 90, 90); path.CloseFigure(); return path;
    }
}

internal sealed class RnButton : Button
{
    private Color _baseColor;
    public RnButton(Color color) { _baseColor = color; BackColor = color; FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; ForeColor = Color.White; Cursor = Cursors.Hand; DoubleBuffered = true; SetStyle(ControlStyles.UserPaint, true); }
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); BackColor = ControlPaint.Light(_baseColor, .12f); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); BackColor = _baseColor; }
    protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); BackColor = ControlPaint.Dark(_baseColor, .08f); }
    protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); BackColor = ClientRectangle.Contains(PointToClient(Cursor.Position)) ? ControlPaint.Light(_baseColor, .12f) : _baseColor; }
    protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; using var path = RnCard.RoundedPath(ClientRectangle, 8); using var brush = new SolidBrush(BackColor); e.Graphics.FillPath(brush, path); TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter); }
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
