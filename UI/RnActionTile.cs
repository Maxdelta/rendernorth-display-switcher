namespace RenderNorth.DisplaySwitcher.UI;
internal sealed class RnActionTile : RnButton { public RnActionTile(string text, EventHandler action) : base(Color.FromArgb(44,54,60)) { Text = text; AutoSize = true; MinimumSize = new Size(150, LayoutTokens.ButtonHeight); Click += action; } }
