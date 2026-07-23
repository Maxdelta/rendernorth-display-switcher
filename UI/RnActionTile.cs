using System.Drawing.Drawing2D;

namespace RenderNorth.DisplaySwitcher.UI;

internal sealed class RnActionTile : RnButton
{
    private const int CompactHeight = 48;

    public RnActionTile(string text, EventHandler action) : base(RnTheme.Control)
    {
        Text = text;
        AccentColor = RnTheme.Accent;
        AutoSize = false;
        MinimumSize = new Size(0, CompactHeight);
        Font = new Font("Segoe UI Semibold", 8.5f);
        Click += action;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SetClip(ClientRectangle);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.Clear(RnTheme.OpaqueAncestorBackColor(this));

        var scale = DeviceDpi / 96f;
        var bounds = new Rectangle(1, 1, Math.Max(1, ClientSize.Width - 3), Math.Max(1, ClientSize.Height - 3));
        using var path = RnCard.RoundedPath(bounds, Math.Max(6, (int)Math.Round(8 * scale)));
        using var fill = new SolidBrush(BackColor);
        using var border = new Pen(Color.FromArgb(112, AccentColor));
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);

        var parts = Text.Split(["  "], 2, StringSplitOptions.None);
        var inset = Math.Max(5, (int)Math.Round(6 * scale));
        var iconSide = Math.Max(26, (int)Math.Round(28 * scale));
        var iconBounds = new Rectangle(
            bounds.Left + inset,
            bounds.Top + Math.Max(0, (bounds.Height - iconSide) / 2),
            iconSide,
            iconSide);
        var labelGap = Math.Max(3, (int)Math.Round(4 * scale));
        var labelBounds = new Rectangle(
            iconBounds.Right + labelGap,
            bounds.Top,
            Math.Max(1, bounds.Right - iconBounds.Right - labelGap - inset),
            bounds.Height);

        using var iconWash = new SolidBrush(Color.FromArgb(32, AccentColor));
        using var iconBorder = new Pen(Color.FromArgb(128, AccentColor));
        e.Graphics.FillEllipse(iconWash, iconBounds);
        e.Graphics.DrawEllipse(iconBorder, iconBounds);
        TextRenderer.DrawText(
            e.Graphics,
            parts[0],
            Font,
            iconBounds,
            AccentColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        TextRenderer.DrawText(
            e.Graphics,
            parts.Length == 2 ? parts[1] : Text,
            Font,
            labelBounds,
            ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

        if (Focused && ShowFocusCues)
        {
            var focusBounds = Rectangle.Inflate(bounds, -3, -3);
            using var focusPath = RnCard.RoundedPath(focusBounds, Math.Max(4, (int)Math.Round(6 * scale)));
            using var focusPen = new Pen(AccentColor) { DashStyle = DashStyle.Dot };
            e.Graphics.DrawPath(focusPen, focusPath);
        }
    }
}
