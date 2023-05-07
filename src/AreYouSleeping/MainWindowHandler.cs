using Microsoft.Extensions.DependencyInjection;

namespace AreYouSleeping;

public static class MainWindowHandler
{
    public static MainWindow? Instance { get; set; } = null;


    public static void ShowOrFocus()
    {
        if (Instance == null)
        {
            var mainWindow = ((App)App.Current).ServiceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();
        }
        else
        {
            Instance.Focus();
        }
    }
}
