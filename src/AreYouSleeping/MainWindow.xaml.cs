using Microsoft.Extensions.Logging;
using System;
using System.Windows;

namespace AreYouSleeping;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public MainWindow(ILogger<MainWindow> logger)
    {
        logger.LogWarning("DI is working, and NLog is working :)");

        MainWindowHandler.Instance = this;
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        MainWindowHandler.Instance = null;
        base.OnClosed(e);
    }
}
