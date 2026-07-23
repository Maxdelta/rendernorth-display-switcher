using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using RenderNorth.DisplaySwitcher.Domain.Modules;
using RenderNorth.DisplaySwitcher.Services;
using RenderNorth.DisplaySwitcher.UI;
using Velopack;
using Xunit;

namespace RenderNorth.Environments.Tests;

public sealed class EnvironmentListLayoutTests
{
    [Fact]
    public void SavedEnvironmentCardHasVisibleBoundsAfterRefresh()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var folder = Path.Combine(Path.GetTempPath(), "RenderNorthEnvironmentListTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            try
            {
                VelopackApp.Build().Run();
                var repository = new EnvironmentRepository(Path.Combine(folder, "environments.json"));
                var manager = new EnvironmentManager(repository, new EnvironmentModuleRegistry());
                Assert.True(manager.Create("Coding").Success);
                var log = new AppLogger();
                using var form = new MainForm(
                    manager,
                    new UpdateService(log),
                    log,
                    () => new ModuleDocument(),
                    () => "",
                    () => { },
                    new ShortcutService(
                        Path.Combine(folder, "RenderNorthDisplaySwitcher.exe"),
                        Path.Combine(folder, "shortcuts.json")));

                form.ClientSize = new Size(900, 760);
                form.CreateControl();
                var refresh = typeof(MainForm).GetMethod("RefreshAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new MissingMethodException(nameof(MainForm), "RefreshAsync");
                ((Task)refresh.Invoke(form, null)!).GetAwaiter().GetResult();
                form.PerformLayout();

                var listField = typeof(MainForm).GetField("_environmentList", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? throw new MissingFieldException(nameof(MainForm), "_environmentList");
                var list = (TableLayoutPanel)listField.GetValue(form)!;
                list.PerformLayout();

                Assert.Equal(2, list.Controls.Count);
                Assert.True(list.Height > 0);
                Assert.All(list.Controls.Cast<Control>(), control =>
                {
                    Assert.True(control.Width > 0);
                    Assert.True(control.Height > 0);
                });
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
