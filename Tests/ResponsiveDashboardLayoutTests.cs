using RenderNorth.DisplaySwitcher.UI;
using System.Drawing;
using System.Windows.Forms;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class ResponsiveDashboardLayoutTests
{
    [Fact]
    public void EmptyStateAndQuickActionsRemainVisibleAcrossRepeatedResizes()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new Form();
                using var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
                using var dashboard = new TableLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    ColumnCount = 1,
                    RowCount = 2
                };
                using var emptyState = new RnEmptyState(() => { }, () => { });
                using var quickActions = new RnCreatePanel((_, _) => { }, (_, _) => { }, (_, _) => { }, (_, _) => { });

                dashboard.Controls.Add(emptyState, 0, 0);
                dashboard.Controls.Add(quickActions, 0, 1);
                scroll.Controls.Add(dashboard);
                form.Controls.Add(scroll);
                form.CreateControl();
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                form.Show();

                foreach (var size in new[]
                {
                    new Size(900, 790),
                    new Size(1640, 920),
                    new Size(900, 790),
                    new Size(820, 700),
                    new Size(1200, 850),
                    new Size(900, 790)
                })
                {
                    form.ClientSize = size;
                    form.PerformLayout();

                    var availableWidth = Math.Max(1, scroll.ClientSize.Width - scroll.Padding.Horizontal -
                        (scroll.VerticalScroll.Visible ? SystemInformation.VerticalScrollBarWidth : 0));
                    dashboard.MinimumSize = new Size(availableWidth, 0);
                    dashboard.MaximumSize = new Size(availableWidth, 0);
                    emptyState.ApplyAvailableWidth(availableWidth);
                    quickActions.ApplyAvailableWidth(availableWidth);
                    dashboard.PerformLayout();
                    emptyState.PerformLayout();
                    quickActions.PerformLayout();

                    Assert.True(emptyState.ContentHost.Visible);
                    Assert.True(emptyState.ContentHost.Width > 0);
                    Assert.True(emptyState.ContentHost.Height > 0);
                    Assert.True(IsInside(emptyState, emptyState.ContentHost));
                    Assert.All(new[] { "Gaming", "Development", "Streaming", "Presentation", "Travel" },
                        category => Assert.Contains(AllControls(emptyState).OfType<Button>(), button => button.Text.Contains(category, StringComparison.Ordinal)));
                    Assert.Contains(AllControls(emptyState).OfType<Button>(), button => button.Text == "Capture Current Setup");
                    Assert.Contains(AllControls(emptyState).OfType<Button>(), button => button.Text == "New Environment");
                    Assert.True(quickActions.Visible);
                    Assert.True(quickActions.Bottom <= dashboard.DisplayRectangle.Bottom);
                    Assert.True(dashboard.Width <= scroll.DisplayRectangle.Width);
                    Assert.False(scroll.HorizontalScroll.Visible);
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private static bool IsInside(Control ancestor, Control descendant)
    {
        var screenBounds = descendant.RectangleToScreen(descendant.ClientRectangle);
        var localBounds = ancestor.RectangleToClient(screenBounds);
        return ancestor.DisplayRectangle.Contains(localBounds);
    }

    private static IEnumerable<Control> AllControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var descendant in AllControls(child))
                yield return descendant;
        }
    }
}
