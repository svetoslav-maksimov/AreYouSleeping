using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Windows;
using NLog;
using NLog.Extensions.Logging;
using AreYouSleeping.Automation;

namespace AreYouSleeping;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider? ServiceProvider { get; private set; }

    public IConfiguration? Configuration { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        SetupLocalization();

        // system tray setup
        var icon = new System.Windows.Forms.NotifyIcon
        {
            Icon = ProjectResources.SysTrayIcon,
            Visible = true
        };

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Configure", null, Configure_Clicked));
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Exit", null, Exit_Clicked));

        icon.ContextMenuStrip = contextMenu;
        icon.DoubleClick += Icon_DoubleClick;

        // DI Setup

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();

        var serviceCollection = new ServiceCollection();

        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    public void SetupLocalization(string culture = "en-US")
    {
        var languageDictionary = new ResourceDictionary();
        switch (culture)
        {
            case "bg-BG":
                languageDictionary.Source = new Uri("/Resources/StringResources.bg-BG.xaml", UriKind.Relative);
                break;

            case "en-US":
            default:
                languageDictionary.Source = new Uri("/Resources/StringResources.xaml", UriKind.Relative);
                break;
        }

        Resources.MergedDictionaries.Clear();
        Resources.MergedDictionaries.Add(languageDictionary);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        LogManager.Shutdown();
        base.OnExit(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AppSettings>(Configuration!.GetSection(nameof(AppSettings))!);
        services.AddLogging(loggingBuilder =>
        {
            // configure Logging with NLog
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
            loggingBuilder.AddNLog();
        });

        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<BrowserAutomation>();
        services.AddSingleton<ShutdownAutomation>();
        services.AddSingleton<AwakePromptFactory>();
        services.AddTransient<MainWindow>();
    }

    private void Icon_DoubleClick(object? sender, EventArgs e)
    {
        MainWindowHandler.ShowOrFocus();
    }

    private void Configure_Clicked(object? sender, EventArgs e)
    {
        MainWindowHandler.ShowOrFocus();
    }

    private void Exit_Clicked(object? sender, EventArgs e)
    {
        this.Shutdown();
    }
}
