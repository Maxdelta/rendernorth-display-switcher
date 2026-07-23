namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnActionTile : RnButton { public RnActionTile(string text, EventHandler action) : base(RnTheme.Control) { Text = text; AccentColor = RnTheme.Accent; AutoSize = true; MinimumSize = new Size(150, LayoutTokens.ButtonHeight); Click += action; } }
